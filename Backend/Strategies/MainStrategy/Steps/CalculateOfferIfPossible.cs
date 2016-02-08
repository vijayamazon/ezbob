namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Backend.Strategies.PricingModel;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using EZBob.DatabaseLib.Model.Database.Loans;

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
			int notHomeOwnerCap,
			int smallLoanScenarioLimit,
			bool aspireToMinSetupFee
		) : base(outerContextDescription) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
			this.nlCashRequestID = nlCashRequestID;
			this.tag = tag;
			this.autoRejectionOutput = autoRejectionOutput ?? new AutoRejectionOutput();
			this.requestedLoan = requestedLoan;
			this.homeOwnerCap = homeOwnerCap;
			this.notHomeOwnerCap = notHomeOwnerCap;
			this.smallLoanScenarioLimit = smallLoanScenarioLimit;
			this.aspireToMinSetupFee = aspireToMinSetupFee;

			OfferResult = null;
			ProposedAmount = 0;
			this.loanSource = null;

			this.gradeID = null;
			this.subGradeID = null;

			this.allLoanSources = ((LoanSourceName[])Enum.GetValues(typeof(LoanSourceName))).Select(x => (int)x).ToArray();
		} // constructor

		[StepOutput]
		public MedalResult Medal { get { return this.medalAgent == null ? null : this.medalAgent.Result; } }

		[StepOutput]
		public OfferResult OfferResult { get; private set; }

		[StepOutput]
		public int LoanSourceID { get { return this.loanSource == null ? 0 : (int)this.loanSource.Value; } }

		[StepOutput]
		public int ProposedAmount { get; private set; }

		public override string Outcome { get { return this.outcome; } }

		protected override StepResults Run() {
			if (this.autoRejectionOutput == null) {
				Log.Alert("No auto rejection output specified for {0}, auto decision is aborted.", OuterContextDescription);

				this.outcome = "'failure - no auto rejection executed'";
				return StepResults.Failed;
			} // if

			if (this.autoRejectionOutput.FlowType == AutoDecisionFlowTypes.Unknown) {
				Log.Alert("Illegal flow type specified for {0}, auto decision is aborted.", OuterContextDescription);

				this.outcome = "'failure - illegal flow type'";
				return StepResults.Failed;
			} // if

			CalculateMedal();

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

			if (OfferResult != null) {
				Log.Msg("Offer proposed for {0}:\n{1}.", OuterContextDescription, OfferResult);
				OfferResult.SaveToDb(DB);
			} // if

			bool failure =
				(OfferResult == null) ||
				OfferResult.IsError ||
				OfferResult.IsMismatch ||
				!OfferResult.HasDecision ||
				(LoanSourceID <= 0);

			if (failure) {
				this.outcome = "'failure'";
				return StepResults.Failed;
			} // if

			this.outcome = "'success'";
			return StepResults.Success;
		} // Run

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
				CreateErrorResult(
					"Failed to load grade range and product subtype by grade range id {0} and product sub type id {1}.",
					this.autoRejectionOutput.GradeRangeID,
					this.autoRejectionOutput.ProductSubTypeID
				);

				return;
			} // if

			GradeRangeSubproduct grsp = sr.Fill<GradeRangeSubproduct>();

			this.gradeID = grsp.GradeID;
			this.subGradeID = grsp.SubGradeID;

			ProposedAmount = GetLogicalProposedAmount(grsp);

			if (ProposedAmount <= 0) {
				CreateErrorResult("Proposed amount is not positive for {0}, no offer.", OuterContextDescription);
				return;
			} // if

			this.loanSource = this.allLoanSources.Contains(grsp.LoanSourceID)
				? (LoanSourceName)grsp.LoanSourceID
				: (LoanSourceName?)null;

			if (this.loanSource == null) {
				CreateErrorResult("Failed to detect default loan source for {0}, no offer.", OuterContextDescription);
				return;
			} // if

			this.repaymentPeriod = grsp.Term(this.requestedLoan.RequestedTerm);

			this.loanTypeID = grsp.LoanTypeID;

			this.minInterestRate = grsp.MinInterestRate;
			this.maxInterestRate = grsp.MaxInterestRate;

			this.minSetupFee = grsp.MinSetupFee;
			this.maxSetupFee = grsp.MaxSetupFee;

			Calculate();
		} // CreateLogicalOffer

		private int GetLogicalProposedAmount(GradeRangeSubproduct grsp) {
			decimal annualTurnover = 0;
			decimal freeCashFlow = 0;
			decimal valueAdded = 0;

			try {
				var turnoverCalc = new TurnoverCalculator(this.customerID, DateTime.UtcNow, DB, Log).LoadInputData();

				if (turnoverCalc.OutOfDate)
					Log.Debug("Out of date marketplaces detected for {0}.", OuterContextDescription);
				else {
					turnoverCalc.Execute();

					if (turnoverCalc.HasOnline) {
						Log.Debug("Online turnover for {0}.", OuterContextDescription);
						turnoverCalc.ExecuteOnline();
					} else
						Log.Debug("Offline turnover for {0}.", OuterContextDescription);

					annualTurnover = turnoverCalc.Model.AnnualTurnover;
					freeCashFlow = turnoverCalc.Model.UseHmrc ? turnoverCalc.Model.FreeCashFlowValue : 0;
					valueAdded = turnoverCalc.Model.UseHmrc ? turnoverCalc.Model.ValueAdded : 0;
				} // if
			} catch (Exception e) {
				Log.Alert(e, "Failed to load turnover for {0}.", OuterContextDescription);
			} // try

			Log.Debug(
				"Turnover = {0}, FCF = {1}, VA = {2} for {3}.",
				annualTurnover,
				freeCashFlow,
				valueAdded,
				OuterContextDescription
			);

			decimal[] allOffers = {
				annualTurnover * (grsp.TurnoverShare ?? 0),
				freeCashFlow * (grsp.FreeCashFlowShare ?? 0),
				valueAdded * (grsp.ValueAddedShare ?? 0),
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
		} // GetLogicalProposedAmount

		private void CreateUnlogicalOffer() {
			if ((this.medalAgent == null) || this.medalAgent.HasError || (Medal == null)) {
				CreateErrorResult("Medal calculation failed.");
				return;
			} // if

			this.medalAgent.Notify();

			if (Medal.HasError) {
				CreateErrorResult("Error calculating medal.");
				return;
			} // if

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

			int loanSourceID = sr.IsEmpty ? 0 : sr["LoanSourceID"];

			this.loanSource = this.allLoanSources.Contains(loanSourceID)
				? (LoanSourceName)loanSourceID
				: (LoanSourceName?)null;

			bool noGo =
				sr.IsEmpty ||
				(ProposedAmount <= 0) ||
				(LoanSourceID <= 0) ||
				(Medal.MedalClassification == EZBob.DatabaseLib.Model.Database.Medal.NoClassification);

			if (noGo) {
				string errorMsg = string.Join(
					" ",
					new List<string> {
						(sr.IsEmpty || (LoanSourceID <= 0)) ? "Failed to detect default loan source ID." : null,
						ProposedAmount <= 0 ? "Proposed amount is not positive." : null,
						(Medal.MedalClassification == EZBob.DatabaseLib.Model.Database.Medal.NoClassification)
							? "No medal calculated."
							: null,
					}.Where(s => !string.IsNullOrWhiteSpace(s))
				);

				CreateErrorResult("'{0}' for {1}, no offer.", errorMsg, OuterContextDescription);
				return;
			} // if

			this.repaymentPeriod = sr["RepaymentPeriod"] ?? 15;

			sr = DB.GetFirst("GetLoanTypeAndDefault", CommandSpecies.StoredProcedure, new QueryParameter("@LoanTypeID"));

			this.loanTypeID = sr.IsEmpty ? 0 : sr["DefaultLoanTypeID"];

			if (this.loanTypeID <= 0) {
				CreateErrorResult("Default loan type not detected for {0}, no offer.", OuterContextDescription);
				return;
			} // if

			sr = DB.GetFirst(
				"AV_OfferInterestRateRange",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Medal", Medal.MedalClassification.ToString())
			);

			if (sr.IsEmpty) {
				CreateErrorResult(
					"Failed to load medal {0} interest rate range for {1}.",
					Medal.MedalClassification,
					OuterContextDescription
				);
				return;
			} // if

			this.minInterestRate = sr["MinInterestRate"] / 100.0M;
			this.maxInterestRate = sr["MaxInterestRate"] / 100.0M;

			sr = DB.GetFirst(
				"LoadOfferRanges",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Amount", ProposedAmount),
				new QueryParameter("@IsNewLoan", Medal.NumOfLoans < 1)
			);

			if (sr.IsEmpty) {
				CreateErrorResult(
					"Failed to load {0} amount of {1} set up fee range for {2}.",
					Medal.NumOfLoans > 0 ? "repeating" : "new",
					ProposedAmount,
					OuterContextDescription
				);
				return;
			} // if

			this.minSetupFee = sr["MinSetupFee"];
			this.maxSetupFee = sr["MaxSetupFee"];

			Calculate();
		} // CreateUnlogicalOffer

		private void Calculate() {
			var calculatorModel = InitCalcualtorModel();

			if (calculatorModel == null)
				return;

			var calculator = new PricingModelCalculator(this.customerID, calculatorModel) {
				ThrowExceptionOnError = false,
				CalculateApr = false,
				TargetLoanSource = (LoanSourceName)LoanSourceID
			};
			calculator.Execute();

			if (!string.IsNullOrWhiteSpace(calculator.Error)) {
				CreateErrorResult("Calculator error '{0}' for {1}, no offer.", calculator.Error, OuterContextDescription);
				return;
			} // if

			PricingSourceModel calculatorOutput = (calculatorModel.PricingSourceModels == null)
				? null
				: calculatorModel.PricingSourceModels.FirstOrDefault(
					r => r.IsPreferable && ((int)r.LoanSource == LoanSourceID)
				);

			if (calculatorOutput == null) {
				CreateErrorResult(
					"Calculator output not found for loan source '{0}' for {1}, no offer.",
					LoanSourceID,
					OuterContextDescription
				);
				return;
			} // if

			CreateOfferResult();
			OfferResult.InterestRate = calculatorOutput.InterestRate;
			OfferResult.SetupFee = calculatorOutput.SetupFee * 100.0M;

			if ((this.minInterestRate <= OfferResult.InterestRate) && (OfferResult.InterestRate <= this.maxInterestRate)) {
				OfferResult.InterestRate *= 100.0M;
				return;
			} // if

			if ((this.minSetupFee == this.maxSetupFee) || !this.aspireToMinSetupFee)
				OfferResult.InterestRate = this.minInterestRate * 100.0M;
			else
				OfferResult.InterestRate = this.maxInterestRate * 100.0M;
		} // Calculate

		private PricingModelModel InitCalcualtorModel() {
			PricingCalcuatorScenarioNames scenarioName;

			int loanCount = LoadLoanCount();

			if (ProposedAmount <= this.smallLoanScenarioLimit)
				scenarioName = PricingCalcuatorScenarioNames.SmallLoan;
			else if (loanCount < 1)
				scenarioName = PricingCalcuatorScenarioNames.BasicNew;
			else
				scenarioName = PricingCalcuatorScenarioNames.BasicRepeating;

			var generator = new GetPricingModelModel(this.customerID, scenarioName) { LoadFromLastCashRequest = false, };
			generator.Execute();

			if (!string.IsNullOrWhiteSpace(generator.Error)) {
				CreateErrorResult(
					"Init calculator model error '{0}' for {1}, no offer.",
					generator.Error,
					OuterContextDescription
				);
				return null;
			} // if

			generator.Model.LoanAmount = ProposedAmount;
			generator.Model.SetupFeePercents = this.aspireToMinSetupFee ? this.minSetupFee : this.maxSetupFee;
			generator.Model.LoanTerm = this.repaymentPeriod;

			return generator.Model;
		} // InitCalculatorModel

		private int LoadLoanCount() {
			if (this.autoRejectionOutput.FlowType == AutoDecisionFlowTypes.LogicalGlue) {
				try {
					return DB.ExecuteScalar<int>(
						"GetCustomerLoanCount",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@CustomerID", this.customerID),
						new QueryParameter("@Now", DateTime.UtcNow)
					);
				} catch (Exception e) {
					Log.Alert(e, "Failed to load loan count for {0}.", OuterContextDescription);
					return 0;
				} // try
			} // if

			return ((this.medalAgent == null) || this.medalAgent.HasError || (Medal == null) || Medal.HasError)
				? 0
				: Medal.NumOfLoans;
		} // LoadLoanCount

		private void CreateErrorResult(string format, params object[] args) {
			CreateOfferResult(string.Format(format, args));
		} // CreateErrorResult

		private void CreateOfferResult(string errorMsg = null) {
			bool hasError = !string.IsNullOrWhiteSpace(errorMsg);

			if (hasError)
				Log.Warn("{0}", errorMsg);

			OfferResult = new OfferResult {
				CustomerId = this.customerID,
				CalculationTime = DateTime.UtcNow,
				Amount = ProposedAmount,
				MedalClassification = Medal == null
					? EZBob.DatabaseLib.Model.Database.Medal.NoClassification
					: Medal.MedalClassification,
				FlowType = this.autoRejectionOutput.FlowType,
				GradeID = this.gradeID,
				SubGradeID = this.subGradeID,
				CashRequestID = this.cashRequestID,
				NLCashRequestID = this.nlCashRequestID,

				ScenarioName = this.autoRejectionOutput.FlowType.ToString(),
				Period = this.repaymentPeriod,
				LoanTypeId = this.loanTypeID,
				LoanSourceId = LoanSourceID,
				Message = hasError ? errorMsg.Trim() : null,
				IsError = hasError,
				IsMismatch = false,
				HasDecision = !hasError,
			};
		} // CreateOfferResult

		// Input parameters.
		private readonly int customerID;
		private readonly long cashRequestID;
		private readonly long nlCashRequestID;
		private readonly string tag;
		private readonly AutoRejectionOutput autoRejectionOutput;
		private readonly MonthlyRepaymentData requestedLoan;
		private readonly int homeOwnerCap;
		private readonly int notHomeOwnerCap;
		private readonly int smallLoanScenarioLimit;
		private readonly bool aspireToMinSetupFee;

		// Intermediate data + holds medal result (output).
		private CalculateMedal medalAgent;

		// Intermediate data filled differently by Logical Glue/Internal flows.
		// Used as pricing calculator input (and some of them - as output).
		private decimal minInterestRate;
		private decimal maxInterestRate;
		private decimal minSetupFee;
		private decimal maxSetupFee;
		private int repaymentPeriod;
		private LoanSourceName? loanSource;
		private int loanTypeID;
		private int? gradeID;
		private int? subGradeID;

		// Output.
		private string outcome;

		// Internal "constant".
		private readonly int[] allLoanSources;
	} // class CalculateOfferIfPossible
} // namespace
