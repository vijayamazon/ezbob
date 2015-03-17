﻿namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	internal abstract class AMedalAndPricing {
		public int LoanCount { get; private set; }
		[NonTraversable]
		public DateTime DecisionTime { get; set; }
		[NonTraversable]
		public string Decision { get; set; }

		[NonTraversable]
		public string MedalName { get; set; }

		[NonTraversable]
		public decimal? EzbobScore { get; set; }

		public string DecisionStr {
			get {
				switch (Decision) {
				case "approved":
					return "Approved";

				case "ApprovedPending":
					return "Pending";

				case "not approved":
					return "Manual";

				default:
					return Decision;
				} // switch
			} // get
		} // DecisionStr

		public decimal Amount { get; set; }
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
		public int RepaymentPeriod { get; set; }

		public void Calculate(int customerID, bool isHomeOwner, AConnection db, ASafeLog log) {
			var instance = new CalculateMedal(customerID, DecisionTime, true, false);
			instance.Execute();

			MedalName = instance.Result.MedalClassification.ToString();

			EzbobScore = instance.Result.TotalScoreNormalized;

			log.Debug("Before capping the offer: {0}", instance.Result);

			int amount = Math.Min(
				instance.Result.RoundOfferedAmount(),
				isHomeOwner
					? CurrentValues.Instance.MaxCapHomeOwner
					: CurrentValues.Instance.MaxCapNotHomeOwner
			);

			var approveAgent = new AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent(
				customerID,
				amount,
				(AutomationCalculator.Common.Medal)instance.Result.MedalClassification,
				(AutomationCalculator.Common.MedalType)instance.Result.MedalType,
				(AutomationCalculator.Common.TurnoverType?)instance.Result.TurnoverType,
				DecisionTime,
				db,
				log
			).Init();
			approveAgent.MakeDecision();

			Amount = amount;
			Decision = approveAgent.Trail.GetDecisionName();

			if (amount == 0) {
				RepaymentPeriod = 0;
				InterestRate = 0;
				SetupFee = 0;
			} else {
				var odc = new OfferDualCalculator(
					customerID,
					DecisionTime,
					amount,
					LoanCount > 0,
					instance.Result.MedalClassification
				);

				odc.CalculateOffer();

				RepaymentPeriod = odc.VerifyBoundaries.RepaymentPeriod;
				InterestRate = odc.VerifyBoundaries.InterestRate / 100.0m;
				SetupFee = odc.VerifyBoundaries.SetupFee / 100.0m;
			} // if
		} // Calculate

		public void LoadLoans(int customerID, AConnection db) {
			LoanCount = db.ExecuteScalar<int>(
				"GetCustomerLoanCount",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("Now", DecisionTime)
			);
		} // LoadLoans

		public static string CsvTitles(string prefix) {
			return string.Format(
				"{0} Decision time;{0} Medal;{0} Ezbob Score;{0} Decision;{0} Amount;{0} Interest Rate;" +
				"{0} Repayment Period;{0} Setup Fee %;{0} Setup Fee Amount",
				prefix
			);
		} // ToCsv

		public string ToCsv() {
			return string.Join(";",
				DecisionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				MedalName,
				EzbobScore,
				DecisionStr,
				Amount.ToString(CultureInfo.InvariantCulture),
				InterestRate.ToString(CultureInfo.InvariantCulture),
				RepaymentPeriod.ToString(CultureInfo.InvariantCulture),
				SetupFeePct.ToString(CultureInfo.InvariantCulture),
				SetupFeeAmount.ToString(CultureInfo.InvariantCulture)
			);
		} // ToCsv

		protected abstract decimal SetupFeePct { get; }
		protected abstract decimal SetupFeeAmount { get; }
	} // class MedalAndPricing
} // namespace