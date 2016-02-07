namespace AutomationCalculator.MedalCalculation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.Turnover;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class TurnoverCalculator {
		public TurnoverCalculator(int customerID, DateTime calculationDate, AConnection db, ASafeLog log) {
			this.db = db;
			this.log = log.Safe();

			Model = new MedalInputModel {
				CustomerId = customerID,
				CalculationDate = calculationDate,
			};
		} // constructor

		public MedalInputModel Model { get; private set; }

		public int MinApprovalAmount { get; private set; }

		/// <summary>
		/// Determines which medal to calculate and calculates it.
		/// Which medal type to choose logic:
		/// https://drive.google.com/open?id=0B1Io_qu9i44SVzVqV19nbnMxRW8&amp;authuser=0
		/// </summary>
		/// <returns>Calculated medal type.</returns>
		public MedalType GetMedalType(out string errorMsg) {
			errorMsg = null;

			var medalChooserData = this.db.FillFirst<MedalChooserInputModelDb>(
				"AV_GetMedalChooserInputParams",
				new QueryParameter("@CustomerId", Model.CustomerId),
				new QueryParameter("@Now", Model.CalculationDate)
			);

			bool hmrcTooOld = AccountIsTooOld(
				Model.CalculationDate,
				medalChooserData.HasHmrc,
				medalChooserData.LastHmrcUpdateDate,
				medalChooserData.MedalDaysOfMpRelevancy
			);

			if (hmrcTooOld) {
				errorMsg =  "Hmrc data is too old";
				return MedalType.NoMedal;

			} // if

			bool bankTooOld = AccountIsTooOld(
				Model.CalculationDate,
				medalChooserData.HasBank,
				medalChooserData.LastBankUpdateDate,
				medalChooserData.MedalDaysOfMpRelevancy
			);

			if (bankTooOld) {
				errorMsg = "Bank data is too old";
				return MedalType.NoMedal;
			} // if

			var type = MedalType.NoMedal;

			if (medalChooserData.IsLimited)
				type = medalChooserData.HasOnline ? MedalType.OnlineLimited : MedalType.Limited;
			else if (medalChooserData.HasCompanyScore)
				type = medalChooserData.HasOnline ? MedalType.OnlineNonLimitedWithBusinessScore : MedalType.NonLimited;
			else if (medalChooserData.HasOnline)
				type = MedalType.OnlineNonLimitedNoBusinessScore;

			bool isSoleTrader = type == MedalType.NoMedal &&
				medalChooserData.HasPersonalScore &&
				(medalChooserData.HasBank || medalChooserData.HasHmrc);

			if (isSoleTrader)
				type = MedalType.SoleTrader;

			MinApprovalAmount = medalChooserData.MinApprovalAmount;

			return type;
		} // GetMedal

		public void Execute() {
			MedalInputModelDb dbData = this.db.FillFirst<MedalInputModelDb>(
				"AV_GetMedalInputParams",
				new QueryParameter("@CustomerId", Model.CustomerId),
				new QueryParameter("@Now", Model.CalculationDate)
			);

			Model.MedalInputModelDb = dbData;

			Model.HasHmrc = dbData.HasHmrc;
			Model.UseHmrc = dbData.HasHmrc;

			// --------start new turnover calculation for medal ----------------//
			//	Flow: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
			Model.TurnoverType = null;
			Model.Turnovers = new List<TurnoverDbRow>();

			this.db.ForEachResult<TurnoverDbRow>(
				r => {
					Model.Turnovers.Add(r);
					r.WriteToLog(this.log);
				},
				"GetCustomerTurnoverForAutoDecision",
				new QueryParameter("IsForApprove", true),
				new QueryParameter("CustomerID", Model.CustomerId),
				new QueryParameter("Now", Model.CalculationDate)
			);

			if (Model.HasHmrc) {
				var hmrcList = (
					from TurnoverDbRow r in Model.Turnovers
					where r.MpTypeID.Equals(MpType.Hmrc)
					select r
				).AsQueryable();

				decimal hmrcTurnover = hmrcList.Sum(t => t.Turnover);
				Model.HmrcAnnualTurnover = (hmrcTurnover < 0) ? 0 : hmrcTurnover;

				Model.AnnualTurnover = Model.HmrcAnnualTurnover;
				Model.TurnoverType = TurnoverType.HMRC;
			} // if has HMRC
			
			if (dbData.NumOfBanks > 0) {
				var yodleeList = (
					from TurnoverDbRow r in Model.Turnovers
					where r.MpTypeID.Equals(MpType.Yodlee)
					select r
				).AsQueryable();

				decimal yoodleeTurnover = yodleeList.Sum(t => t.Turnover);
				Model.YodleeAnnualTurnover = (yoodleeTurnover < 0) ? 0 : yoodleeTurnover;

				if (Model.TurnoverType.Equals(null)) {
					Model.AnnualTurnover = Model.YodleeAnnualTurnover;
					Model.TurnoverType = TurnoverType.Bank;
				} // if
			} // if has bank

			// --------end new turnover calculation for medal----------------//

			Model.FreeCashFlowValue = 0;
			Model.ValueAdded = 0;

			decimal newActualLoansRepayment = 0;

			this.db.ForEachRowSafe(
				srfv => {
					RowType rt;

					if (!Enum.TryParse(srfv["RowType"], out rt)) {
						this.log.Alert(
							"TurnoverCalculator.Execute: Cannot parse row type from {0}",
							srfv["RowType"]
						);
						return;
					} // if

					switch (rt) {
					case RowType.FcfValueAdded:
						Model.FreeCashFlowValue += srfv["FreeCashFlow"];
						Model.ValueAdded += srfv["ValueAdded"];
						break;

					case RowType.NewActualLoansRepayment:
						newActualLoansRepayment = srfv["NewActualLoansRepayment"];
						break;

					default:
						throw new ArgumentOutOfRangeException();
					} // switch
				},
				"GetCustomerAnnualFcfValueAdded",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Model.CustomerId),
				new QueryParameter("Now", Model.CalculationDate)
			);

			Model.FreeCashFlowValue -= newActualLoansRepayment;

			Model.FreeCashFlow = Model.AnnualTurnover == 0 || !dbData.HasHmrc
				? 0
				: Model.FreeCashFlowValue / Model.AnnualTurnover;

			Model.TangibleEquity = Model.AnnualTurnover == 0
				? 0
				: Model.MedalInputModelDb.TangibleEquity / Model.AnnualTurnover;
		} // Execute

		/// <summary>
		/// Online turnover for medal: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
		/// Also sets FreeCashFlow, TangibleEquity, NumOfStores, PositiveFeedbacks, UseHmrc.
		/// </summary>
		public void ExecuteOnline() {
			GetOnlineTurnover();

			Model.FreeCashFlow = ((Model.AnnualTurnover == 0) || !Model.HasHmrc)
				? 0
				: Model.FreeCashFlowValue / Model.AnnualTurnover;

			Model.TangibleEquity = (Model.AnnualTurnover == 0)
				? 0
				: Model.MedalInputModelDb.TangibleEquity / Model.AnnualTurnover;

			Model.NumOfStores = Model.MedalInputModelDb.NumOfStores;

			Model.UseHmrc = (Model.TurnoverType == TurnoverType.HMRC);
		} // ExecuteOnline

		/// <summary>
		/// Online turnover for medal: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
		/// </summary>
		private void GetOnlineTurnover() {
			try {
				decimal[] months = new decimal[3];
				decimal[] quarters = new decimal[3];
				decimal[] halves = new decimal[3];
				decimal[] fulls = new decimal[3];

				Model.FilterTurnoversTo(MpType.Amazon, months, quarters, halves, fulls);
				Model.FilterTurnoversTo(MpType.Ebay, months, quarters, halves, fulls);
				Model.FilterTurnoversTo(MpType.PayPal, months, quarters, halves, fulls);

				LogMarketplaceTurnovers("month", months);
				LogMarketplaceTurnovers("quarter", quarters);
				LogMarketplaceTurnovers("half year", halves);
				LogMarketplaceTurnovers("year", fulls);

				// Online turnover: Amazon + MAX(eBay, Pay Pal)

				var onlineList = new List<decimal> {
					MaxTurnover(months),
					MaxTurnover(quarters),
					MaxTurnover(halves),
					MaxTurnover(fulls),
				};

				this.log.Debug("Online turnover list: {0}", string.Join("; ", onlineList));

				Model.OnlineAnnualTurnover = onlineList.Where(r => r > 0).DefaultIfEmpty(0).Min();

				Model.OnlineAnnualTurnover = (Model.OnlineAnnualTurnover < 0) ? 0 : Model.OnlineAnnualTurnover;

				decimal onlineTurnoverEdge = Model.OnlineAnnualTurnover * Model.MedalInputModelDb.OnlineMedalTurnoverCutoff;

				string medalTypeMsg;

				if (Model.HmrcAnnualTurnover > onlineTurnoverEdge) {
					Model.AnnualTurnover = Model.HmrcAnnualTurnover;
					Model.TurnoverType = TurnoverType.HMRC;
					medalTypeMsg = "HmrcAnnualTurnover-><-onlineMedalTurnoverCutoff";
				} else if (Model.YodleeAnnualTurnover > onlineTurnoverEdge) {
					Model.AnnualTurnover = Model.YodleeAnnualTurnover;
					Model.TurnoverType = TurnoverType.Bank;
					medalTypeMsg = "BankAnnualTurnover-><-onlineMedalTurnoverCutoff";
				} else {
					Model.AnnualTurnover = Model.OnlineAnnualTurnover;
					Model.TurnoverType = TurnoverType.Online;
					medalTypeMsg = "Online";
				} // if

				this.log.Info(
					"AV finally: ({5}) " +
					"AnnualTurnover: {0}, " +
					"HmrcAnnualTurnover: {1}, " +
					"YodleeAnnualTurnover: {2}, " +
					"OnlineAnnualTurnover: {3}, " +
					"Type: {4}",
					Model.AnnualTurnover,
					Model.HmrcAnnualTurnover,
					Model.YodleeAnnualTurnover,
					Model.OnlineAnnualTurnover,
					Model.TurnoverType,
					medalTypeMsg
				);
			} catch (Exception ex) {
				this.log.Error(ex, "Failed to calculate online annual turnover for medal.");
			} // try
		} // GetOnlineTurnover

		private void LogMarketplaceTurnovers(string periodName, decimal[] data) {
			this.log.Debug(
				"Turnovers for {0}: Amazon: {1}, eBay: {2}, Pay Pal: {3}",
				periodName,
				data[Amazon],
				data[Ebay],
				data[PayPal]
			);
		} // LogMarketplaceTurnovers

		private static decimal MaxTurnover(decimal[] list) {
			return list[Amazon] + Math.Max(list[Ebay], list[PayPal]);
		} // MaxTurnover

		private static int Amazon { get { return TurnoverCalculatorExt.Amazon; } }
		private static int Ebay { get { return TurnoverCalculatorExt.Ebay; } }
		private static int PayPal { get { return TurnoverCalculatorExt.PayPal; } }

		private enum RowType {
			NewActualLoansRepayment,
			FcfValueAdded,
		} // enum RowType

		private readonly AConnection db;
		private readonly ASafeLog log;

		private static bool AccountIsTooOld(DateTime today, bool hasAccounts, DateTime? lastUpdated, int threshold) {
			if (!hasAccounts)
				return false;

			if (lastUpdated == null)
				return true;

			return (today - lastUpdated.Value).TotalDays > threshold;
		} // AccountIsTooOld
	} // class TurnoverCalculator

	internal static class TurnoverCalculatorExt {
		internal const int Amazon = 0;
		internal const int Ebay = 1;
		internal const int PayPal = 2;

		internal static void FilterTurnoversTo(
			this MedalInputModel model,
			Guid mpTypeID,
			decimal[] months,
			decimal[] quarters,
			decimal[] halves,
			decimal[] fulls
		) {
			int idx = mpIndices[mpTypeID];

			List<TurnoverDbRow> sourceList = model.Turnovers.Where(r => r.MpTypeID == mpTypeID).ToList();

			months.Add(idx, sourceList, PartOfYear.Month);
			quarters.Add(idx, sourceList, PartOfYear.Quarter);
			halves.Add(idx, sourceList, PartOfYear.Half);
			fulls.Add(idx, sourceList, PartOfYear.Full);
		} // FilterTurnoversTo

		private enum PartOfYear { 
			Month = 1,
			Quarter = 3,
			Half = 6,
			Full = 12,
		} // enum PartOfYear

		private static void Add(
			this decimal[] targetList,
			int idx,
			IEnumerable<TurnoverDbRow> sourceList,
			PartOfYear partOfYear
		) {
			int monthAfter = (int)partOfYear;
			int coef = 12 / monthAfter;

			targetList[idx] = coef * sourceList.Where(t => (t.Distance < monthAfter)).Sum(t => t.Turnover);
		} // Add

		private static readonly Dictionary<Guid, int> mpIndices = new Dictionary<Guid, int> {
			{ MpType.Amazon, Amazon },
			{ MpType.Ebay, Ebay },
			{ MpType.PayPal, PayPal },
		};
	} // class TurnoverCalculatorExt
} // namespace
