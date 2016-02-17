﻿namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
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

			this.medalAgent = new CalculateMedal(
				this.customerID,
				this.cashRequestID,
				this.nlCashRequestID,
				DateTime.UtcNow,
				false,
				true
			) { Tag = tag, };
		} // constructor

		[StepOutput]
		public MedalResult Medal { get { return this.medalAgent.Result; } }

		[StepOutput]
		public OfferResult OfferResult { get; private set; }

		[StepOutput]
		public int LoanSourceID { get { return this.loanSource == null ? 0 : (int)this.loanSource.Value; } }

		[StepOutput]
		public int ProposedAmount { get; private set; }

		public override string Outcome { get { return this.outcome; } }

		protected override StepResults Run() {
			if (this.autoRejectionOutput.FlowType == AutoDecisionFlowTypes.Unknown) {
				Log.Alert("Illegal flow type specified for {0}, auto decision is aborted.", OuterContextDescription);

				this.outcome = "'failure - illegal flow type'";
				return StepResults.Failed;
			} // if

			this.medalAgent.Execute();

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

			GradeRangeSubproduct grsp = LoadOfferRanges(true);

			if (grsp == null)
				return;

			ProposedAmount = GetLogicalProposedAmount(grsp);

			if (ProposedAmount <= 0) {
				CreateErrorResult("Proposed amount is not positive for {0}, no offer.", OuterContextDescription);
				return;
			} // if

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
			this.medalAgent.Notify();

			if (Medal.HasError) {
				CreateErrorResult("Error calculating medal for {0}.", OuterContextDescription);
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

			bool noGo =
				(ProposedAmount <= 0) ||
				(Medal.MedalClassification == EZBob.DatabaseLib.Model.Database.Medal.NoClassification);

			if (noGo) {
				string errorMsg = string.Join(
					" ",
					new List<string> {
						ProposedAmount <= 0 ? "Proposed amount is not positive." : null,
						(Medal.MedalClassification == EZBob.DatabaseLib.Model.Database.Medal.NoClassification)
							? "No medal calculated."
							: null,
					}.Where(s => !string.IsNullOrWhiteSpace(s))
				);

				CreateErrorResult("'{0}' for {1}, no offer.", errorMsg, OuterContextDescription);
				return;
			} // if

			GradeRangeSubproduct grsp = LoadOfferRanges(false);

			if (grsp == null)
				return;

			Calculate();
		} // CreateUnlogicalOffer

		private GradeRangeSubproduct LoadOfferRanges(bool isLogicalOffer) {
			var loader = isLogicalOffer
				? new LoadOfferRanges(
					this.autoRejectionOutput.GradeRangeID,
					this.autoRejectionOutput.ProductSubTypeID,
					DB,
					Log
				)
				: new LoadOfferRanges(this.customerID, null, DateTime.UtcNow, DB, Log);
			
			loader.Execute();

			if (!loader.Success) {
				CreateErrorResult(
					"Failed to load default loan type or loan parameters ranges {0}, no offer.",
					OuterContextDescription
				);
				return null;
			} // if

			this.repaymentPeriod = loader.GradeRangeSubproduct.Term(this.requestedLoan.RequestedTerm);
			this.gradeID = loader.GradeRangeSubproduct.GradeID;
			this.subGradeID = loader.GradeRangeSubproduct.SubGradeID;
			this.loanTypeID = loader.GradeRangeSubproduct.LoanTypeID;
			this.minInterestRate = loader.GradeRangeSubproduct.MinInterestRate;
			this.maxInterestRate = loader.GradeRangeSubproduct.MinInterestRate;
			this.minSetupFee = loader.GradeRangeSubproduct.MinSetupFee;
			this.maxSetupFee = loader.GradeRangeSubproduct.MaxSetupFee;

			this.loanSource = this.allLoanSources.Contains(loader.GradeRangeSubproduct.LoanSourceID)
				? (LoanSourceName)loader.GradeRangeSubproduct.LoanSourceID
				: (LoanSourceName?)null;

			if (this.loanSource == null) {
				CreateErrorResult("Failed to detect default loan source for {0}, no offer.", OuterContextDescription);
				return null;
			} // if

			return loader.GradeRangeSubproduct;
		} // LoadOfferRanges

		private void Calculate() {
			var calc = new OfferCalculator(
				this.autoRejectionOutput.FlowType,
				this.customerID,
				LoanSourceID,
				ProposedAmount,
				this.repaymentPeriod,
				LoadLoanCount() > 0,
				this.aspireToMinSetupFee,
				this.smallLoanScenarioLimit,
				this.minInterestRate,
				this.maxInterestRate,
				this.minSetupFee,
				this.maxSetupFee,
				Log
			).Calculate();

			if (!calc.Success) {
				CreateErrorResult("Calculator error for {0}, no offer.", OuterContextDescription);
				return;
			} // if

			CreateOfferResult();
			OfferResult.InterestRate = calc.InterestRate * 100.0M;
			OfferResult.SetupFee = calc.SetupFee * 100.0M;
		} // Calculate

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

			return Medal.HasError ? 0 : Medal.NumOfLoans;
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
		private readonly AutoRejectionOutput autoRejectionOutput;
		private readonly MonthlyRepaymentData requestedLoan;
		private readonly int homeOwnerCap;
		private readonly int notHomeOwnerCap;
		private readonly int smallLoanScenarioLimit;
		private readonly bool aspireToMinSetupFee;

		// Intermediate data + holds medal result (output).
		private readonly CalculateMedal medalAgent;

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
