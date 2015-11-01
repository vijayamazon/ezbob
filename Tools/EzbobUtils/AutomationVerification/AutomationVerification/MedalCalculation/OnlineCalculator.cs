namespace AutomationCalculator.MedalCalculation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.Turnover;
	using Ezbob.Database;
	using Ezbob.Logger;

	public abstract class OnlineCalculator : MedalCalculator {
		public override MedalInputModel GetInputParameters(int customerId, DateTime calculationDate) {
			var model = base.GetInputParameters(customerId, calculationDate);
			model = GetOnlineInputParameters(customerId, model);
			return model;
		} // GetInputParameters

		protected OnlineCalculator(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		/// <summary>
		/// Online turnover for medal: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
		/// Also sets FreeCashFlow, TangibleEquity, NumOfStores, PositiveFeedbacks, UseHmrc.
		/// </summary>
		/// <param name="customerId"></param>
		/// <param name="intputModel"></param>
		/// <returns></returns>
		protected MedalInputModel GetOnlineInputParameters(int customerId, MedalInputModel intputModel) {
			intputModel = GetOnlineTurnover(intputModel);

			intputModel.FreeCashFlow = ((intputModel.AnnualTurnover == 0) || !intputModel.HasHmrc)
				? 0
				: intputModel.FreeCashFlowValue / intputModel.AnnualTurnover;

			intputModel.TangibleEquity = (intputModel.AnnualTurnover == 0)
				? 0
				: intputModel.MedalInputModelDb.TangibleEquity / intputModel.AnnualTurnover;

			intputModel.NumOfStores = intputModel.MedalInputModelDb.NumOfStores;

			intputModel.PositiveFeedbacks = LoadFeedbacks(customerId);
			intputModel.UseHmrc = (intputModel.TurnoverType == TurnoverType.HMRC);

			return intputModel;
		} // GetOnlineInputParameters

		/// <summary>
		/// Online turnover for medal: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private MedalInputModel GetOnlineTurnover(MedalInputModel model) {
			try {
				decimal[] months = new decimal[3];
				decimal[] quarters = new decimal[3];
				decimal[] halves = new decimal[3];
				decimal[] fulls = new decimal[3];

				model.FilterTurnoversTo(MpType.Amazon, months, quarters, halves, fulls);
				model.FilterTurnoversTo(MpType.Ebay, months, quarters, halves, fulls);
				model.FilterTurnoversTo(MpType.PayPal, months, quarters, halves, fulls);

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

				this.Log.Debug("Online turnover list: {0}", string.Join("; ", onlineList));

				model.OnlineAnnualTurnover = onlineList.Where(r => r > 0).DefaultIfEmpty(0).Min();

				model.OnlineAnnualTurnover = (model.OnlineAnnualTurnover < 0) ? 0 : model.OnlineAnnualTurnover;

				decimal onlineTurnoverEdge = model.OnlineAnnualTurnover * model.MedalInputModelDb.OnlineMedalTurnoverCutoff;

				string medalTypeMsg;

				if (model.HmrcAnnualTurnover > onlineTurnoverEdge) {
					model.AnnualTurnover = model.HmrcAnnualTurnover;
					model.TurnoverType = TurnoverType.HMRC;
					medalTypeMsg = "HmrcAnnualTurnover-><-onlineMedalTurnoverCutoff";
				} else if (model.YodleeAnnualTurnover > onlineTurnoverEdge) {
					model.AnnualTurnover = model.YodleeAnnualTurnover;
					model.TurnoverType = TurnoverType.Bank;
					medalTypeMsg = "BankAnnualTurnover-><-onlineMedalTurnoverCutoff";
				} else {
					model.AnnualTurnover = model.OnlineAnnualTurnover;
					model.TurnoverType = TurnoverType.Online;
					medalTypeMsg = "Online";
				} // if

				this.Log.Info(
					"AV finally: ({5}) " +
					"AnnualTurnover: {0}, " +
					"HmrcAnnualTurnover: {1}, " +
					"YodleeAnnualTurnover: {2}, " +
					"OnlineAnnualTurnover: {3}, " +
					"Type: {4}",
					model.AnnualTurnover,
					model.HmrcAnnualTurnover,
					model.YodleeAnnualTurnover,
					model.OnlineAnnualTurnover,
					model.TurnoverType,
					medalTypeMsg
				);

				return model;
			} catch (Exception ex) {
				this.Log.Error(ex, "Failed to calculate online annual turnover for medal.");
			} // try

			return model;
		} // GetOnlineTurnover

		private void LogMarketplaceTurnovers(string periodName, decimal[] data) {
			this.Log.Debug(
				"Turnovers for {0}: Amazon: {1}, eBay: {2}, Pay Pal: {3}",
				periodName,
				data[Amazon],
				data[Ebay],
				data[PayPal]
			);
		} // LogMarketplaceTurnovers

		private int LoadFeedbacks(int customerId) {
			var feedbacksDb = this.DB.FillFirst<PositiveFeedbacksModelDb>(
				"AV_GetFeedbacks",
				new QueryParameter("@CustomerId", customerId)
			);

			int feedbacks = feedbacksDb.AmazonFeedbacks + feedbacksDb.EbayFeedbacks;

			if (feedbacks == 0)
				feedbacks = feedbacksDb.PaypalFeedbacks;

			if (feedbacks == 0)
				feedbacks = feedbacksDb.DefaultFeedbacks;

			this.Log.Debug(
				"Secondary medal - positive feedbacks:\n" +
				"\tAmazon: {0}\n\teBay: {1}\n\tPay Pal: {2}\n\tDefault: {3}\n\tFinal: {4}",
				feedbacksDb.AmazonFeedbacks,
				feedbacksDb.EbayFeedbacks,
				feedbacksDb.PaypalFeedbacks,
				feedbacksDb.DefaultFeedbacks,
				feedbacks
			);

			return feedbacks;
		} // LoadFeedbacks

		private static decimal MaxTurnover(decimal[] list) {
			return list[Amazon] + Math.Max(list[Ebay], list[PayPal]);
		} // MaxTurnover

		private static int Amazon { get { return OnlineCalculatorExt.Amazon; } }
		private static int Ebay { get { return OnlineCalculatorExt.Ebay; } }
		private static int PayPal { get { return OnlineCalculatorExt.PayPal; } }
	} // class OnlineCalculator

	internal static class OnlineCalculatorExt {
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
	} // class OnlineCalculatorExt
} // namespace
