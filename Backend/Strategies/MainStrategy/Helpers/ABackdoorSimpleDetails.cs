namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using System.Threading;
	using AutomationCalculator.Common;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib.Model.Database;

	internal abstract class ABackdoorSimpleDetails {
		public static ABackdoorSimpleDetails Create(
			int customerID,
			string email,
			bool ownsProperty,
			decimal requestedAmount,
			int homeOwnerCap,
			int notHomeOwnerCap,
			int delay
		) {
			if (string.IsNullOrWhiteSpace(email)) {
				Log.Alert(
					"Not using back door simple for customer '{0}': customer email not specified.",
					customerID
				);

				return null;
			} // if

			int atPos = email.LastIndexOf('@');

			if (atPos < 0) {
				Log.Alert(
					"Not using back door simple for customer '{0}': customer email '{1}' contains no '@'.",
					customerID,
					email
				);

				return null;
			} // if

			int plusPos = email.LastIndexOf('+', atPos - 1);

			if (plusPos < 0) {
				Log.Debug(
					"Not using back door simple for customer '{0}': customer email '{1}' contains no '+'.",
					customerID,
					email
				);

				return null;
			} // if

			string backdoorCode = email.Substring(plusPos + 1, atPos - plusPos - 1);

			ABackdoorSimpleDetails result =
				BackdoorSimpleReject.Create(backdoorCode, customerID, ownsProperty, requestedAmount, homeOwnerCap, notHomeOwnerCap, delay) ??
				BackdoorSimpleApprove.Create(backdoorCode, customerID, ownsProperty, requestedAmount, homeOwnerCap, notHomeOwnerCap, delay) ??
				BackdoorSimpleManual.Create(backdoorCode, customerID, ownsProperty, requestedAmount, homeOwnerCap, notHomeOwnerCap, delay);

			if (result != null) {
				Log.Debug("Using back door simple for customer '{0}' as: {1}.", customerID, result);
				return result;
			} // if

			Log.Debug(
				"Not using back door simple for customer '{0}': back door code '{1}' from customer email '{2}' " +
				"ain't no matches any existing back door regex.",
				customerID,
				backdoorCode,
				email
			);

			return null;
		} // Create

		public MedalResult Medal { get; private set; }
		public OfferResult OfferResult { get; private set; }

		public DecisionActions Decision { get; private set; }

		public int Delay { get; private set; }

		public int ApprovedAmount { get; private set; }

		public abstract bool SetResult(AutoDecisionResponse response);

		public void SetAdditionalCustomerData(
			long acashRequestID,
			long anlCashRequestID,
			int asmallLoanScenarioLimit,
			bool aaspireToMinSetupFee,
			TypeOfBusiness atypeOfBusiness,
			int acustomerOriginID,
			MonthlyRepaymentData arequestedLoan,
			int aofferValidHours
		) {
			this.cashRequestID = acashRequestID;
			this.nlCashRequestID = anlCashRequestID;
			this.smallLoanScenarioLimit = asmallLoanScenarioLimit;
			this.aspireToMinSetupFee = aaspireToMinSetupFee;
			this.typeOfBusiness = atypeOfBusiness;
			this.customerOriginID = acustomerOriginID;
			this.requestedLoan = arequestedLoan;
			this.offerValidHours = aofferValidHours;
		} // SetAdditionalCustomerData

		protected ABackdoorSimpleDetails(
			int homeOwnerCap,
			int notHomeOwnerCap,
			int customerID,
			DecisionActions decision,
			int delay,
			bool ownsProperty,
			decimal requestedAmount,
			bool gradeMode = false
		) {
			this.customerID = customerID;
			Decision = decision;
			Delay = delay;
			this.ownsProperty = ownsProperty;
			this.homeOwnerCap = homeOwnerCap;
			this.notHomeOwnerCap = notHomeOwnerCap;

			Medal = null;
			OfferResult = null;

			int appAmount = (int)(requestedAmount / 1000m);

			ApprovedAmount = MedalResult.RoundOfferedAmount(gradeMode ? appAmount * 1000 : Cap(appAmount * 1000));
		} // constructor

		protected static ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log

		protected static AConnection DB {
			get { return Library.Instance.DB; }
		} // DB

		protected virtual void DoDelay() {
			if (Delay < 1)
				return;

			// fix for too small delay configuration
			if (Delay < 20)
				Delay = 20;

			Log.Debug(
				"Back door simple flow: delaying for {0}...",
				Grammar.Number(Delay, "second")
			);

			for (int i = 0; i < Delay; i++) {
				Thread.Sleep(1000);

				Log.Debug(
					"Back door simple flow: {0} of {1} delay have passed.",
					Grammar.Number(i + 1, "second"),
					Grammar.Number(Delay, "second")
				);
			} // for each delay second

			Log.Debug(
				"Back door simple flow: delaying for {0} complete.",
				Grammar.Number(Delay, "second")
			);
		} // DoDelay

		protected readonly int customerID;
		protected int offerValidHours;
		protected MonthlyRepaymentData requestedLoan;
		protected int smallLoanScenarioLimit;
		protected bool aspireToMinSetupFee;
		protected TypeOfBusiness typeOfBusiness;
		protected int customerOriginID;

		protected GradeRangeSubproduct Grsp { get; private set; }

		protected virtual bool CalculateMedalAndOffer() {
			Grsp = null;

			var loader = new LoadOfferRanges(this.customerID, null, DateTime.UtcNow, DB, Log).Execute();

			Medal = new MedalResult(
				this.customerID,
				null,
				loader.Success ? null : "Failed to load grade range + sub-product."
			) {
				CalculationTime = DateTime.UtcNow,
				MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification,
				MedalType = Strategies.MedalCalculations.MedalType.NoMedal,
				OfferedLoanAmount = ApprovedAmount,
				AnnualTurnover = ApprovedAmount,
				TotalScoreNormalized = 0,
			};

			if (loader.Success)
				Grsp = loader.GradeRangeSubproduct;

			OfferResult = new OfferResult {
				CustomerId = this.customerID,
				CalculationTime = DateTime.UtcNow,
				Amount = ApprovedAmount,
				MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification,
				FlowType = AutoDecisionFlowTypes.Internal,
				GradeID = null,
				SubGradeID = null,
				CashRequestID = this.cashRequestID,
				NLCashRequestID = this.nlCashRequestID,

				ScenarioName = "Back door - no offer",
				Period = (Grsp != null) ? Grsp.Term(this.requestedLoan.RequestedTerm) : 15,
				LoanTypeId = (Grsp != null) ? Grsp.LoanTypeID : 1,
				LoanSourceId = (Grsp != null) ? Grsp.LoanSourceID : 1,
				InterestRate = 0,
				SetupFee = 0,
				Message = (Grsp != null) ? "Failed to load grade range + sub-product." : null,
				IsError = Grsp == null,
				IsMismatch = false,
				HasDecision = false,
			};

			return loader.Success;
		} // CalculateMedalAndOffer

		private int Cap(int val) {
			return Math.Min(
				val,
				this.ownsProperty ? this.homeOwnerCap : this.notHomeOwnerCap
			);
		} // Cap

		private readonly bool ownsProperty;
		private readonly int homeOwnerCap;
		private readonly int notHomeOwnerCap;

		private long cashRequestID;
		private long nlCashRequestID;
	} // class ABackdoorSimpleDetails
} // namespace
