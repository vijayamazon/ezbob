namespace AutomationCalculator.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Common;
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

		public decimal GetOnlineTurnoverAnnualized(List<MarketPlace> mps)
		{
			//TODO implement
			/*
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
			*/
			return 0;
		}

		public decimal GetYodleeAnnualized(List<MarketPlace> yodlees, ASafeLog log) {
			double incomeAnnualized = 0;
			var dbHelper = new DbHelper(log);
			foreach (var yodlee in yodlees) {
				var afs = dbHelper.GetAnalysisFunctions(yodlee.Id);
				if (!afs.Any())
				{
					continue;
				}

				var av = afs
					.OrderByDescending(af => af.TimePeriod)
					.FirstOrDefault(af => af.Function == "TotalIncomeAnnualized" && af.TimePeriod <= TimePeriodEnum.Year);
				
				if (av != null) {
					incomeAnnualized += av.Value;
				}
			}

			return (decimal)incomeAnnualized;
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
