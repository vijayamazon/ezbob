namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System.Threading;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using AutomationCalculator.Common;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MainStrategy.Steps;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib.Model.Database;

	internal abstract class ABackdoorSimpleDetails {
		public static ABackdoorSimpleDetails Create(int customerID, string email, bool ownsProperty) {
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
				(ABackdoorSimpleDetails)BackdoorSimpleReject.Create(backdoorCode, customerID) ??
				(ABackdoorSimpleDetails)BackdoorSimpleApprove.Create(backdoorCode, customerID, ownsProperty) ??
				(ABackdoorSimpleDetails)BackdoorSimpleManual.Create(backdoorCode, customerID);

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

		public abstract bool SetResult(AutoDecisionResponse response);

		public void SetAdditionalCustomerData(
			string aouterContextDescription,
			long acashRequestID,
			long anlCashRequestID,
			string atag,
			int ahomeOwnerCap,
			int anotHomeOwnerCap,
			int asmallLoanScenarioLimit,
			bool aaspireToMinSetupFee,
			TypeOfBusiness atypeOfBusiness,
			int acustomerOriginID,
			MonthlyRepaymentData arequestedLoan
		) {
			this.outerContextDescription = aouterContextDescription;
			this.cashRequestID = acashRequestID;
			this.nlCashRequestID = anlCashRequestID;
			this.tag = atag;
			this.homeOwnerCap = ahomeOwnerCap;
			this.notHomeOwnerCap = anotHomeOwnerCap;
			this.smallLoanScenarioLimit = asmallLoanScenarioLimit;
			this.aspireToMinSetupFee = aaspireToMinSetupFee;
			this.typeOfBusiness = atypeOfBusiness;
			this.customerOriginID = acustomerOriginID;
			this.requestedLoan = arequestedLoan;
		} // SetAdditionalCustomerData

		protected ABackdoorSimpleDetails(int customerID, DecisionActions decision, int delay) {
			this.customerID = customerID;
			Decision = decision;
			Delay = delay;

			Medal = null;
			OfferResult = null;
		} // constructor

		protected static ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log

		private static AConnection DB {
			get { return Library.Instance.DB; }
		} // DB

		protected virtual void DoDelay() {
			if (Delay < 1)
				return;

			//fix for too small delay configuration
			if (Delay < 20) {
				Delay = 20;
			}
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

		protected virtual bool CalculateMedalAndOffer(decimal? gradeScore, int proposedAmount) {
			SafeReader sr = Library.Instance.DB.GetFirst(
				"GetDefaultLoanSource",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID)
			);

			if (sr.IsEmpty) {
				Log.Warn(
					"Back door simple approval failed for customer '{0}': could not load default loan source.",
					this.customerID
				);
				return false;
			} // if

			int sourceID = sr["LoanSourceID"];

			var matchingGradeRanges = new MatchingGradeRanges();

			if (gradeScore != null) {
				var spRanges = new LoadMatchingGradeRanges(DB, Log) {
					IsFirstLoan = true,
					IsRegulated = this.typeOfBusiness.IsRegulated(),
					LoanSourceID = sourceID,
					OriginID = this.customerOriginID,
					Score = gradeScore.Value,
				};
				spRanges.Execute(matchingGradeRanges);

				if (matchingGradeRanges.Count != 1) {
					Log.Warn(
						"Back door simple approval failed for customer '{0}': {1} found.",
						this.customerID,
						Grammar.Number(matchingGradeRanges.Count, "matching grade range")
					);
					return false;
				} // if

				// TODO: create corresponding fake fetch LG score entries.
			} // if

			var autoRejectionOutput = new AutoRejectionOutput {
				ErrorInLGData = false,
				FlowType = gradeScore == null ? AutoDecisionFlowTypes.Internal : AutoDecisionFlowTypes.LogicalGlue,
				GradeRangeID = gradeScore == null ? 0 : matchingGradeRanges[0].GradeRangeID,
				ProductSubTypeID = gradeScore == null ? 0 : matchingGradeRanges[0].ProductSubTypeID,
			};

			var calc = new CalculateOfferIfPossible(
				this.outerContextDescription,
				this.customerID,
				this.cashRequestID,
				this.nlCashRequestID,
				this.tag,
				autoRejectionOutput,
				this.requestedLoan,
				this.homeOwnerCap,
				this.notHomeOwnerCap,
				this.smallLoanScenarioLimit,
				this.aspireToMinSetupFee
			) { ForcedProposedAmount = proposedAmount, };

			StepResults calcResult = calc.Execute();

			if (calcResult != StepResults.Success) {
				Log.Warn(
					"Back door simple approval failed for customer '{0}': offer calculation failed.",
					this.customerID
				);

				return false;
			} // if

			if (calc.OfferResult == null || calc.OfferResult.IsError) {
				Log.Warn(
					"Back door simple for customer '{0}' - approval failed. Offer result: '{1}'.",
					this.customerID,
					calc.OfferResult != null ? calc.OfferResult.Description : string.Empty
				);
				return false;
			} // if

			Medal = calc.Medal;
			OfferResult = calc.OfferResult;

			return true;
		} // CalcualteMedalAndOffer

		protected readonly int customerID;

		private string outerContextDescription;
		private long cashRequestID;
		private long nlCashRequestID;
		private string tag;
		private MonthlyRepaymentData requestedLoan;
		private int homeOwnerCap;
		private int notHomeOwnerCap;
		private int smallLoanScenarioLimit;
		private bool aspireToMinSetupFee;
		private TypeOfBusiness typeOfBusiness;
		private int customerOriginID;
	} // class ABackdoorSimpleDetails
} // namespace
