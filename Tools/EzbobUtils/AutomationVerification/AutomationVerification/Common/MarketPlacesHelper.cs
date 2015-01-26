namespace AutomationCalculator.Common {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class MarketPlacesHelper {
		public MarketPlacesHelper(AConnection db, ASafeLog log) {
			DB = db;
			Log = log;
		} // constructor

		public int GetPositiveFeedbacks(int customerId) {
			var dbHelper = new DbHelper(DB, Log);
			var feedbacksDb = dbHelper.GetPositiveFeedbacks(customerId);

			// ebay and amazon
			int feedbacks = feedbacksDb.AmazonFeedbacks + feedbacksDb.EbayFeedbacks;

			//if not - paypal transactions
			if (feedbacks == 0)
				feedbacks = feedbacksDb.PaypalFeedbacks;

			// if not - default value
			if (feedbacks == 0)
				feedbacks = feedbacksDb.DefaultFeedbacks;

			return feedbacks;
		} // GetPositiveFeedbacks

		protected readonly ASafeLog Log;
		protected readonly AConnection DB;
	} // class MarketPlaceHelper
} // namespace
