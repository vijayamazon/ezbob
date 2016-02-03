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
			if (this.autoRejectionOutput.FlowType == AutoDecisionFlowTypes.Unknown) {
				Log.Alert("No auto rejection output specified for {0}, auto decision is aborted.", OuterContextDescription);

				this.outcome = "'failure - no auto rejection executed'";
				return StepResults.Failed;
			} // if

			CalculateMedal();

			if ((this.medalAgent == null) || this.medalAgent.HasError) {
				this.outcome = "'failure - medal calculation failed'";
				return StepResults.Failed;
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

			if ((OfferResult == null) || OfferResult.IsError || OfferResult.IsMismatch || (LoanSourceID <= 0)) {
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
				Log.Warn(
					"Failed to load grade range and product subtype by grade range id {0} and product sub type id {1}.",
					this.autoRejectionOutput.GradeRangeID,
					this.autoRejectionOutput.ProductSubTypeID
				);

				return;
			} // if

			GradeRangeSubproduct grsp = sr.Fill<GradeRangeSubproduct>();

			ProposedAmount = GetProposedAmount(grsp);

			int term = grsp.Term(this.requestedLoan.RequestedTerm);

			if (ProposedAmount <= 0) {
				OfferResult = new OfferResult {
					CustomerId = this.customerID,
					CalculationTime = DateTime.UtcNow,
					Amount = ProposedAmount,
					MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification,
					FlowType = AutoDecisionFlowTypes.LogicalGlue,

					ScenarioName = "Logical Glue - no offer",
					Period = term,
					LoanTypeId = grsp.LoanTypeID,
					LoanSourceId = grsp.LoanSourceID,
					InterestRate = 0,
					SetupFee = 0,
					Message = "Proposed amount is not positive.",
					IsError = true,
					IsMismatch = false,
					HasDecision = true,
				};

				return;
			} // if

			LoanSourceID = grsp.LoanSourceID;

			// TODO: execute offer calculator

			OfferResult = new OfferResult {
				CustomerId = this.customerID,
				CalculationTime = DateTime.UtcNow,
				Amount = ProposedAmount,
				MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification,
				FlowType = AutoDecisionFlowTypes.LogicalGlue,

				ScenarioName = "Logical Glue",
				Period = term,
				LoanTypeId = grsp.LoanTypeID,
				LoanSourceId = grsp.LoanSourceID,
				// TODO InterestRate = should be between 0% and 100%
				// TODO SetupFee = should be between 0% and 100%
				Message = null,
				IsError = false,
				IsMismatch = false,
				HasDecision = true,
			};
		} // CreateLogicalOffer

		private int GetProposedAmount(GradeRangeSubproduct grsp) {
			decimal[] allOffers = {
				Medal.AnnualTurnover * (grsp.TurnoverShare ?? 0),
				Medal.UseHmrc() ? Medal.FreeCashFlowValue * (grsp.FreeCashFlowShare ?? 0) : 0,
				Medal.UseHmrc() ? Medal.ValueAdded * (grsp.ValueAddedShare ?? 0) : 0
			};

			List<int> validOffers = allOffers.Where(v => v > 0).Select(grsp.LoanAmount).Where(v => v > 0).ToList();

			Log.Debug(
				"Proposed amounts for {0}:\n\tAll   offer amounts: {1}\n\tValid offer amounts: {2}",
				OuterContextDescription,
				string.Join(", ", allOffers),
				string.Join(", ", validOffers)
			);

			if (validOffers.Count > 0) {
				int minOffer = validOffers.Min();
				Log.Debug("Proposed offer amount for {0} is {1}.", OuterContextDescription, minOffer);
				return minOffer;
			} // if

			Log.Debug("No valid offers found for {0}.", OuterContextDescription);
			return 0;
		} // GetProposedAmount

		private void CreateUnlogicalOffer() {
			bool isHomeOwner = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID)
			);

			ProposedAmount = Math.Min(
				Medal.RoundOfferedAmount(),
				isHomeOwner ? this.homeOwnerCap : this.notHomeOwnerCap
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
					FlowType = AutoDecisionFlowTypes.Internal,

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

			if (OfferResult != null)
				OfferResult.FlowType = AutoDecisionFlowTypes.Internal;
		} // CreateUnlogicalOffer

		private readonly int customerID;
		private readonly long cashRequestID;
		private readonly long nlCashRequestID;
		private readonly string tag;
		private readonly AutoRejectionOutput autoRejectionOutput;
		private readonly MonthlyRepaymentData requestedLoan;
		private readonly int homeOwnerCap;
		private readonly int notHomeOwnerCap;

		private CalculateMedal medalAgent;
		private string outcome;
	} // class CalculateOfferIfPossible
} // namespace
