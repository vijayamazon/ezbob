namespace AutomationCalculator.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Logger;

	public class MarketPlacesHelper
	{
		protected readonly ASafeLog Log;

		public MarketPlacesHelper(ASafeLog log) {
			Log = log;
		}

		
		public int GetMarketPlacesSeniority(List<MarketPlace> mps)
		{
			if (mps.Any() && mps.Any(m => m.OriginationDate.HasValue))
			{
				var date = mps.Where(x => x.OriginationDate.HasValue).Max(x => x.OriginationDate.Value);
				TimeSpan ts = DateTime.Today - date;
				return (int) ts.TotalDays;
			}
			return 0;
		}

		public double GetTurnoverForPeriod(List<MarketPlace> mps, TimePeriodEnum timePeriod)
		{
			double paypal = 0;
			double ebay = 0;
			double sum = 0;
			var dbHelper = new DbHelper(Log);

			foreach (var marketPlace in mps)
			{
				var afs = dbHelper.GetAnalysisFunctions(marketPlace.Id);
				if (!afs.Any())
				{
					continue;
				}
				var av =
					afs.OrderByDescending(af => af.TimePeriod).FirstOrDefault(af => AnalysisFunctionIncome.IncomeFunctions.Contains(af.Function) && af.TimePeriod <= timePeriod);
				double currentTurnover = Convert.ToDouble(av != null ? av.Value : 0);

				if (afs[0].MarketPlaceName == "Pay Pal")
				{
					paypal += currentTurnover;
				}
				else if (afs[0].MarketPlaceName == "eBay")
				{
					ebay += currentTurnover;
				}
				else
				{
					sum += currentTurnover;
				}
			}
			return sum + Math.Max(paypal, ebay);
		}

		/// <summary>
		/// Calculate online turnover: use this formula:
		///Amazon + Max(ebay,paypal)
		///Calculate it for 1M, 3M, 6M (annualize figure) and 1Y
		///Take the minimum between these 4. (The non-zero minimum)
		/// </summary>
		public decimal GetOnlineTurnoverAnnualized(int customerId) {
			var dbHelper = new DbHelper(Log);
			
			var mps = dbHelper.GetCustomerMarketPlaces(customerId);
			decimal paypal = 0;
			decimal ebay = 0;
			decimal amazon = 0;
			foreach (var marketPlace in mps) {
				var annualizedRevenue = dbHelper.GetOnlineAnnaulizedRevenue(marketPlace.Id);
				switch (marketPlace.Type) {
					case "eBay":
						ebay += annualizedRevenue;
						break;
					case "Amazon":
						amazon += annualizedRevenue;
						break;
					case "Pay Pal":
						paypal += annualizedRevenue;
						break;
				}
			}
			return amazon + Math.Max(paypal, ebay);
		}

		public decimal GetYodleeAnnualized(List<MarketPlace> yodlees, ASafeLog log) {
			decimal incomeAnnualized = 0;
			var dbHelper = new DbHelper(log);
			foreach (var yodlee in yodlees) {
				var yodleeRevenues = dbHelper.GetYodleeRevenues(yodlee.Id);
				if (yodleeRevenues.MinDate.HasValue && yodleeRevenues.MaxDate.HasValue) {
					decimal days = (decimal) (yodleeRevenues.MaxDate.Value - yodleeRevenues.MinDate.Value).TotalDays;

					if (days > 60 && days < 90) {
						days = 90; // we get usually 90 of data
					}

					decimal ratio = Math.Abs(days) < 1 ? 1 : 365.0M/days;
					incomeAnnualized += yodleeRevenues.YodleeRevenues*ratio;
				}
			}
			return incomeAnnualized;
		}

		public int GetPositiveFeedbacks(int customerId) {
			var dbHelper = new DbHelper(Log);
			var feedbacksDb = dbHelper.GetPositiveFeedbacks(customerId);

			//ebay and amazon
			int feedbacks = feedbacksDb.AmazonFeedbacks + feedbacksDb.EbayFeedbacks;
			
			//if not - paypal transactions
			if (feedbacks == 0) {
				feedbacks = feedbacksDb.PaypalFeedbacks;
			}

			//if not - default value
			if (feedbacks == 0) {
				feedbacks = feedbacksDb.DefaultFeedbacks;
			}

			return feedbacks;
		}
	}
}
