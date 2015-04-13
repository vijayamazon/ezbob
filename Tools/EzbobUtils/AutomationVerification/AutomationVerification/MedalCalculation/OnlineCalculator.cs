namespace AutomationCalculator.MedalCalculation {
	using System;
	using System.Collections;
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
		///		Online turnover for medal: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
		///		Set also FreeCashFlow, TangibleEquity, NumOfStores, PositiveFeedbacks, UseHmrc
		/// </summary>
		/// <param name="customerId"></param>
		/// <param name="intputModel"></param>
		/// <returns></returns>
		protected MedalInputModel GetOnlineInputParameters(int customerId, MedalInputModel intputModel) {
			intputModel = this.GetOnlineTurnover(intputModel.Turnovers, intputModel);

			intputModel.FreeCashFlow = intputModel.AnnualTurnover == 0 || !intputModel.HasHmrc ? 0 : intputModel.FreeCashFlowValue / intputModel.AnnualTurnover;

			intputModel.TangibleEquity = intputModel.AnnualTurnover == 0 ? 0 : intputModel.MedalInputModelDb.TangibleEquity / intputModel.AnnualTurnover;
			intputModel.NumOfStores = intputModel.MedalInputModelDb.NumOfStores;

			intputModel.PositiveFeedbacks = LoadFeedbacks(customerId);
			intputModel.UseHmrc = (intputModel.TurnoverType == TurnoverType.HMRC);

			return intputModel;
		} // GetOnlineInputParameters

		/// <summary>
		/// 	Online turnover for medal: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
		/// </summary>
		/// <param name="turnovers"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		private MedalInputModel GetOnlineTurnover(List<TurnoverDbRow> turnovers, MedalInputModel model) {
			try {
				const int T1 = 1;
				const int T3 = 3;
				const int T6 = 6;
				const int T12 = 12;

				const int Ec1 = 12;
				const int Ec3 = 4;
				const int Ec6 = 2;
				const int Ec12 = 1;

				List<decimal> list_t1 = new List<decimal>();
				List<decimal> list_t3 = new List<decimal>();
				List<decimal> list_t6 = new List<decimal>();
				List<decimal> list_t12 = new List<decimal>();

				// extact amazon data
				var amazonList = (from TurnoverDbRow r in turnovers where r.MpTypeID.Equals(MpType.Amazon) select r).AsQueryable();

				// calculate "last month", "last three months", "last six months", and "last twelve months"/annualize for amazon
				list_t1.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T1, Ec1));
				list_t3.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T3, Ec3));
				list_t6.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T6, Ec6));
				list_t12.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T12, Ec12));

				// extact ebay data
				var ebayList = (from TurnoverDbRow r in turnovers where r.MpTypeID.Equals(MpType.Ebay) select r).AsQueryable();

				// calculate "last month", "last three months", "last six months", and "last twelve months"/annualize for ebay
				list_t1.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T1, Ec1));
				list_t3.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T3, Ec3));
				list_t6.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T6, Ec6));
				list_t12.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T12, Ec12));

				// extact paypal data
				var paypalList = (from TurnoverDbRow r in turnovers where r.MpTypeID.Equals(MpType.PayPal) select r).AsQueryable();
	
				// calculate "last month", "last three months", "last six months", and "last twelve months"/annualize for paypal
				list_t1.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T1, Ec1));
				list_t3.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T3, Ec3));
				list_t6.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T6, Ec6));
				list_t12.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T12, Ec12));

				// Online turnover: Amazon + MAX(eBay, Pay Pal)

				// amazon: index 0
				// ebay: index 1
				// paypal: index 2

				var onlineList = new ArrayList();

				onlineList.Add(list_t1.ElementAt(0) + Math.Max(list_t1.ElementAt(1), list_t1.ElementAt(2)));
				onlineList.Add(list_t3.ElementAt(0) + Math.Max(list_t3.ElementAt(1), list_t3.ElementAt(2)));
				onlineList.Add(list_t6.ElementAt(0) + Math.Max(list_t6.ElementAt(1), list_t6.ElementAt(2)));
				onlineList.Add(list_t12.ElementAt(0) + Math.Max(list_t12.ElementAt(1), list_t12.ElementAt(2)));

				model.OnlineAnnualTurnover = (from decimal r in onlineList where r > 0 select r).AsQueryable().DefaultIfEmpty(0).Min();
				model.OnlineAnnualTurnover = (model.OnlineAnnualTurnover < 0) ? 0 : model.OnlineAnnualTurnover;

				if (model.HmrcAnnualTurnover > model.OnlineAnnualTurnover * model.MedalInputModelDb.OnlineMedalTurnoverCutoff) {
					//usingHmrc = true;
					model.AnnualTurnover = model.HmrcAnnualTurnover;
					model.TurnoverType = TurnoverType.HMRC;

					Log.Info("AV: (HmrcAnnualTurnover-><-onlineMedalTurnoverCutoff): AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}", model.AnnualTurnover, model.HmrcAnnualTurnover, model.YodleeAnnualTurnover, model.OnlineAnnualTurnover, model.TurnoverType);

					return model;
				} // if

				if (model.YodleeAnnualTurnover > model.OnlineAnnualTurnover * model.MedalInputModelDb.OnlineMedalTurnoverCutoff) {
					model.AnnualTurnover = model.YodleeAnnualTurnover;
					model.TurnoverType = TurnoverType.Bank;

					Log.Info("AV: (BankAnnualTurnover-><-onlineMedalTurnoverCutoff): AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}", model.AnnualTurnover, model.HmrcAnnualTurnover, model.YodleeAnnualTurnover, model.OnlineAnnualTurnover, model.TurnoverType);

					return model;
				} // if

				model.AnnualTurnover = model.OnlineAnnualTurnover;
				model.TurnoverType = TurnoverType.Online;

				this.Log.Info("AV finally: AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", model.AnnualTurnover, model.HmrcAnnualTurnover, model.YodleeAnnualTurnover, model.OnlineAnnualTurnover, model.TurnoverType);

				return model;
			} catch (Exception ex) {
				this.Log.Error(ex, "Failed to calculate online annual turnover for medal");
			} // try

			return model;
		} // GetOnlineTurnover

		/// <summary>
		///     Calculates for "last month", "last three months", "last six months", and "last twelve months".
		///     Annualize the figures and take the minimum among them.
		/// </summary>
		/// <param name="list"></param>
		/// <param name="monthsBefore"></param>
		/// <param name="extrapolationCoefficient"></param>
		/// <returns></returns>
		private decimal CalcAnnualTurnoverBasedOnPartialData(IQueryable<TurnoverDbRow> list, int monthAfter, int extrapolationCoefficient) {
			return list.Where(t => (t.Distance < monthAfter)).Sum(t => t.Turnover) * extrapolationCoefficient;
		} // CalcAnnualTurnoverBasedOnPartialData

		private int LoadFeedbacks(int customerId) {
			var feedbacksDb = DB.FillFirst<PositiveFeedbacksModelDb>(
				"AV_GetFeedbacks",
				new QueryParameter("@CustomerId", customerId)
			);

			int feedbacks = feedbacksDb.AmazonFeedbacks + feedbacksDb.EbayFeedbacks;

			if (feedbacks == 0)
				feedbacks = feedbacksDb.PaypalFeedbacks;

			if (feedbacks == 0)
				feedbacks = feedbacksDb.DefaultFeedbacks;

			Log.Debug(
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
	} // class OnlineCalculator
} // namespace
