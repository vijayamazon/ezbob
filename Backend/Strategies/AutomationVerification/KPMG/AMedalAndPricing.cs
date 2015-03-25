namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Strategies.Extensions;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using OfficeOpenXml;

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	public abstract class AMedalAndPricing {
		public int LoanCount { get; set; }

		[NonTraversable]
		public DateTime DecisionTime { get; set; }

		[NonTraversable]
		public string Decision { get; set; }

		public bool IsRejected {
			get { return Decision == "Rejected"; }
		} // IsRejected

		public bool IsApproved {
			get { return Decision.StartsWith("Approve"); }
		} // IsApproved

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

		public void Calculate(
			int customerID,
			bool isHomeOwner,
			Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal,
			bool takeMinOffer,
			long cashRequestID,
			string tag,
			AConnection db,
			ASafeLog log
		) {
			log.Info(
				"RunAutomation-Auto{4}.Calculate(customer {0}, cash request {1}, " +
				"has home '{2}', medal '{3}', take min '{4}') started...",
				customerID,
				cashRequestID,
				isHomeOwner,
				medal.MedalClassification,
				takeMinOffer ? "Min" : "Max"
			);

			MedalName = medal.MedalClassification.ToString();

			EzbobScore = medal.TotalScoreNormalized;

			int amount = takeMinOffer ? medal.RoundOfferedAmount() : medal.RoundMaxOfferedAmount();

			log.Info(
				"RunAutomation-Auto{0}.Calculate() before capping: medal '{1}', amount {2}\n{3}",
				takeMinOffer ? "Min" : "Max",
				MedalName,
				amount,
				medal
			);

			amount = Math.Min(
				amount,
				isHomeOwner ? CurrentValues.Instance.MaxCapHomeOwner : CurrentValues.Instance.MaxCapNotHomeOwner
			);

			log.Info(
				"RunAutomation-Auto{0}.Calculate(), after capping: medal name '{1}', amount {2}, " +
				"medal '{3}', medal type '{4}', turnover type '{5}', decision time '{6}'.",
				takeMinOffer ? "Min" : "Max",
				MedalName,
				amount,
				(AutomationCalculator.Common.Medal)medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
				DecisionTime.MomentStr()
			);

			var approveAgent = new AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent(
				customerID,
				amount,
				(AutomationCalculator.Common.Medal)medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
				DecisionTime,
				db,
				log
			).Init();
			approveAgent.MakeDecision();

			approveAgent.Trail.Save(db, null, cashRequestID, tag);

			Amount = amount;
			Decision = approveAgent.Trail.GetDecisionName();

			if (amount == 0) {
				log.Info(
					"RunAutomation-Auto{0}.Calculate(), after decision: amount {1} - not calculating offer.",
					takeMinOffer ? "Min" : "Max",
					amount
				);

				RepaymentPeriod = 0;
				InterestRate = 0;
				SetupFee = 0;
			} else {
				log.Info(
					"RunAutomation-Auto{0}.Calculate(), after decision: amount {1}, " +
					"loan count {2}, medal '{3}', decision time '{4}' - going for offer calculation.",
					takeMinOffer ? "Min" : "Max",
					amount,
					LoanCount,
					medal.MedalClassification,
					DecisionTime.MomentStr()
				);

				var odc = new OfferDualCalculator(
					customerID,
					DecisionTime,
					amount,
					LoanCount > 0,
					medal.MedalClassification
				);

				odc.CalculateOffer();

				RepaymentPeriod = odc.VerifyBoundaries.RepaymentPeriod;
				InterestRate = odc.VerifyBoundaries.InterestRate / 100.0m;
				SetupFee = odc.VerifyBoundaries.SetupFee / 100.0m;

				log.Info(
					"RunAutomation-Auto{0}.Calculate(), offer: amount {1}, " +
					"repayment period {2}, interest rate {3}, setup fee {4}.",
					takeMinOffer ? "Min" : "Max",
					amount,
					RepaymentPeriod,
					InterestRate,
					SetupFee
				);
			} // if

			log.Info(
				"RunAutomation-Auto{4}.Calculate(customer {0}, cash request {1}, " +
				"has home '{2}', medal '{3}', take min '{4}') complete.",
				customerID,
				cashRequestID,
				isHomeOwner,
				medal.MedalClassification,
				takeMinOffer ? "Min" : "Max"
			);
		} // Calculate

		public static string CsvTitles(string prefix) {
			return string.Format(
				"{0} Medal;{0} Ezbob Score;{0} Decision;{0} Amount;{0} Interest Rate;" +
				"{0} Repayment Period;{0} Setup Fee %;{0} Setup Fee Amount",
				prefix
			);
		} // ToCsv

		public string ToCsv() {
			return string.Join(";",
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

		public int ToXlsx(ExcelWorksheet sheet, int rowNum, int colNum) {
			colNum = sheet.SetCellValue(rowNum, colNum, MedalName);
			colNum = sheet.SetCellValue(rowNum, colNum, EzbobScore);
			colNum = sheet.SetCellValue(rowNum, colNum, DecisionStr);
			colNum = sheet.SetCellValue(rowNum, colNum, Amount);
			colNum = sheet.SetCellValue(rowNum, colNum, InterestRate);
			colNum = sheet.SetCellValue(rowNum, colNum, RepaymentPeriod);
			colNum = sheet.SetCellValue(rowNum, colNum, SetupFeePct);
			colNum = sheet.SetCellValue(rowNum, colNum, SetupFeeAmount);
			return colNum;
		} // ToCsv

		protected abstract decimal SetupFeePct { get; }
		protected abstract decimal SetupFeeAmount { get; }
	} // class MedalAndPricing
} // namespace
