namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using OfficeOpenXml;

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	public abstract class AMedalAndPricing {
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
			MedalName = medal.MedalClassification.ToString();

			EzbobScore = medal.TotalScoreNormalized;

			log.Debug("Before capping the offer: {0}", medal);

			int amount = Math.Min(
				takeMinOffer
					?  medal.RoundOfferedAmount()
					:  medal.RoundMaxOfferedAmount(),
				isHomeOwner
					? CurrentValues.Instance.MaxCapHomeOwner
					: CurrentValues.Instance.MaxCapNotHomeOwner
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
				RepaymentPeriod = 0;
				InterestRate = 0;
				SetupFee = 0;
			} else {
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
