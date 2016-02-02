namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;

	internal class CalculateOfferIfPossible : AMainStrategyStep {
		public CalculateOfferIfPossible(
			string outerContextDescription,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag,
			AutoRejectionOutput autoRejectionOutput,
			MonthlyRepaymentData requestedLoan,
			int homeOwnerCap,
			int notHomeOwnerCap
		) : base(outerContextDescription) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
			this.nlCashRequestID = nlCashRequestID;
			this.tag = tag;
			this.autoRejectionOutput = autoRejectionOutput ?? new AutoRejectionOutput();
			this.requestedLoan = requestedLoan;
			this.homeOwnerCap = homeOwnerCap;
			this.notHomeOwnerCap = notHomeOwnerCap;

			OfferResult = null;
			LoanSourceID = 0;
			ProposedAmount = 0;
		} // constructor

		[StepOutput]
		public MedalResult Medal { get { return this.medalAgent == null ? null : this.medalAgent.Result; } }

		[StepOutput]
		public OfferResult OfferResult { get; private set; }

		[StepOutput]
		public int LoanSourceID { get; private set; }

		[StepOutput]
		public int ProposedAmount { get; private set; }

		protected override StepResults Run() {
			this.logicalGlueFlowFailed = false;
			bool rejectWasExecuted = true;

			CalculateMedal();

			this.isHomeOwner = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID)
			);

			if (this.autoRejectionOutput.FlowType == AutoDecisionFlowTypes.Unknown) {
				Log.Alert("No auto rejection output specified for {0} - defaulting to old flow.", OuterContextDescription);

				rejectWasExecuted = false;
				this.autoRejectionOutput.FlowType = AutoDecisionFlowTypes.Internal;
			} // if

			switch (this.autoRejectionOutput.FlowType) {
			case AutoDecisionFlowTypes.LogicalGlue:
				CreateLogicalOffer();
				break;

			case AutoDecisionFlowTypes.Internal:
				CreateUnlogicalOffer();
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch

			bool failure =
				!rejectWasExecuted ||
				(this.medalAgent == null) ||
				this.medalAgent.WasMismatch ||
				(Medal == null) ||
				(Medal.ExceptionDuringCalculation != null) ||
				this.logicalGlueFlowFailed;

			if (failure) {
				this.outcome = "'failure'";
				return StepResults.Failed;
			} // if

			this.outcome = "'success'";
			return StepResults.Success;
		} // Run

		protected override string Outcome { get { return this.outcome; } }

		private void CalculateMedal() {
			this.medalAgent = new CalculateMedal(
				this.customerID,
				this.cashRequestID,
				this.nlCashRequestID,
				DateTime.UtcNow,
				false,
				true
			) {
				Tag = this.tag,
			};

			this.medalAgent.Execute();
		} // CalculateMedal

		private void CreateLogicalOffer() {
			var offerIsImpossibleReasons = new List<string>();

			if (this.autoRejectionOutput.ErrorInLGData)
				offerIsImpossibleReasons.Add("error in LG data");

			if (this.autoRejectionOutput.GradeRangeID <= 0)
				offerIsImpossibleReasons.Add("GradeRangeID is not positive");

			if (this.autoRejectionOutput.ProductSubTypeID <= 0)
				offerIsImpossibleReasons.Add("ProductSubTypeID is not positive");

			if (offerIsImpossibleReasons.Count > 0) {
				Log.Debug(
					"Cannot calculate Logical Glue based offer for {0}: {1}.",
					OuterContextDescription,
					string.Join(", ", offerIsImpossibleReasons)
				);

				return;
			} // if

			SafeReader sr = DB.GetFirst(
				"LoadGradeRangeAndSubproduct",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@GradeRangeID", this.autoRejectionOutput.GradeRangeID),
				new QueryParameter("@ProductSubTypeID", this.autoRejectionOutput.ProductSubTypeID)
			);

			if (sr.IsEmpty) {
				this.logicalGlueFlowFailed = true;

				Log.Warn(
					"Failed to load grade range and product subtype by grade range id {0} and product sub type id {1}.",
					this.autoRejectionOutput.GradeRangeID,
					this.autoRejectionOutput.ProductSubTypeID
				);

				return;
			} // if

			GradeRangeSubproduct grsp = sr.Fill<GradeRangeSubproduct>();

			ProposedAmount = grsp.LoanAmount(this.requestedLoan.RequestedAmount);

			OfferResult = new OfferResult {
				CustomerId = this.customerID,
				CalculationTime = DateTime.UtcNow,
				Amount = ProposedAmount,
				MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification,

				ScenarioName = "Logical Glue",
				Period = grsp.Term(this.requestedLoan.RequestedTerm),
				LoanTypeId = grsp.LoanTypeID,
				LoanSourceId = grsp.LoanSourceID,
				InterestRate = grsp.InterestRate * 100M,
				SetupFee = grsp.SetupFee * 100M,
				Message = null,
				IsError = false,
				IsMismatch = false,
				HasDecision = true,
			};

			LoanSourceID = grsp.LoanSourceID;
		} // CreateLogicalOffer

		private void CreateUnlogicalOffer() {
			ProposedAmount = Math.Min(
				Medal.RoundOfferedAmount(),
				this.isHomeOwner ? this.homeOwnerCap : this.notHomeOwnerCap
			);

			SafeReader sr = DB.GetFirst(
				"GetDefaultLoanSource",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID)
			);

			if (sr.IsEmpty || (ProposedAmount <= 0)) {
				OfferResult = new OfferResult {
					CustomerId = this.customerID,
					CalculationTime = DateTime.UtcNow,
					Amount = ProposedAmount,
					MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification,

					ScenarioName = "Internal - error occurred",
					Period = 0,
					LoanTypeId = 0,
					LoanSourceId = 0,
					InterestRate = 0,
					SetupFee = 0,
					Message = string.Join(" ", new List<string> {
						sr.IsEmpty ? "Failed to detect default loan source ID." : null,
						ProposedAmount <= 0 ? "Proposed amount is not positive." : null,
					}.Where(s => !string.IsNullOrWhiteSpace(s))),
					IsError = true,
					IsMismatch = false,
					HasDecision = false,
				};

				return;
			} // if

			int loanCount = DB.ExecuteScalar<int>(
				"GetCustomerLoanCount",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			LoanSourceID = sr["LoanSourceID"];
			int repaymentPeriod = sr["RepaymentPeriod"] ?? 15;

			var offerDualCalculator = new OfferDualCalculator(
				this.customerID,
				DateTime.UtcNow,
				ProposedAmount,
				loanCount > 0,
				Medal.MedalClassification,
				LoanSourceID,
				repaymentPeriod
			);

			OfferResult = offerDualCalculator.CalculateOffer();
		} // CreateUnlogicalOffer

		private string outcome;

		private readonly int customerID;
		private readonly long cashRequestID;
		private readonly long nlCashRequestID;
		private readonly string tag;
		private readonly AutoRejectionOutput autoRejectionOutput;
		private readonly MonthlyRepaymentData requestedLoan;
		private readonly int homeOwnerCap;
		private readonly int notHomeOwnerCap;

		private CalculateMedal medalAgent;
		private bool isHomeOwner;
		private bool logicalGlueFlowFailed;
	} // class CalculateOfferIfPossible
} // namespace
