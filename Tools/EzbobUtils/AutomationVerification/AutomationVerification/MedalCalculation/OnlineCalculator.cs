namespace AutomationCalculator.MedalCalculation {
	using System;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	public abstract class OnlineCalculator : MedalCalculator {
		public override MedalInputModel GetInputParameters(int customerId, DateTime calculationDate) {
			base.GetInputParameters(customerId, calculationDate);

			TurnoverCalculator.ExecuteOnline();

			TurnoverCalculator.Model.PositiveFeedbacks = LoadFeedbacks(customerId);

			return TurnoverCalculator.Model;
		} // GetInputParameters

		protected OnlineCalculator(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

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
	} // class OnlineCalculator
} // namespace
