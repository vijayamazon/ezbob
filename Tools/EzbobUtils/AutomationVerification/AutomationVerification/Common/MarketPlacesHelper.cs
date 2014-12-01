namespace AutomationCalculator.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class MarketPlacesHelper
	{
		protected readonly ASafeLog Log;
		protected readonly AConnection DB;

		public MarketPlacesHelper(AConnection db, ASafeLog log) {
			DB = db;
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
			var dbHelper = new DbHelper(DB, Log);

			foreach (var marketPlace in mps)
			{
				var afs = dbHelper.GetAnalysisFunctions(marketPlace.Id);
				if (!afs.Any())
				{
					continue;
				}
				var av =
					afs.OrderByDescending(af => af.TimePeriod).FirstOrDefault(af => AnalysisFunctionIncome.IncomeFunctions.Contains(af.Function) && af.TimePeriod <= timePeriod);
				double currentTurnover = av != null ? Convert.ToDouble( av.Value) : 0;

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
		///Amazon + Max(ebay,paypal) + otherNonPayment
		/// annual turnover:
		///Calculate it for 1M, 3M, 6M, 1Y (annualized figure) and 1Y
		///Take the maximum between these 4.
		/// </summary>
		/// <param name="customerId"></param>
		/// <returns></returns>
		public decimal GetTurnoverAnnualizedForRejecetion(List<MarketPlace> mps)
		{
			var dbHelper = new DbHelper(DB, Log);
			var dict = new Dictionary<MarketPlace, OnlineRevenues>();
			foreach (var marketPlace in mps)
			{
				var afs = dbHelper.GetAnalysisFunctions(marketPlace.Id);
				if (!afs.Any()) {
					continue;
				}

				var oneMonthAnnualized = afs.FirstOrDefault(af => af.TimePeriod == TimePeriodEnum.Month && AnalysisFunctionIncome.IncomeAnnualizedFunctions.Contains(af.Function));
				var threeMonthAnnualized = afs.FirstOrDefault(af => af.TimePeriod == TimePeriodEnum.Month3 && AnalysisFunctionIncome.IncomeAnnualizedFunctions.Contains(af.Function));
				var sixMonthMonthAnnualized = afs.FirstOrDefault(af => af.TimePeriod == TimePeriodEnum.Month6 && AnalysisFunctionIncome.IncomeAnnualizedFunctions.Contains(af.Function));
				var yearAnnualized = afs.FirstOrDefault(af => af.TimePeriod == TimePeriodEnum.Year && AnalysisFunctionIncome.IncomeAnnualizedFunctions.Contains(af.Function));
				var yearNotAnnualized = afs.FirstOrDefault(af => af.TimePeriod == TimePeriodEnum.Year && AnalysisFunctionIncome.IncomeFunctions.Contains(af.Function));

				var onlineRevenues = new OnlineRevenues 
				{
					AnnualizedRevenue1M = oneMonthAnnualized == null ? 0 : (decimal) oneMonthAnnualized.Value,
					AnnualizedRevenue3M = threeMonthAnnualized == null ? 0 : (decimal) threeMonthAnnualized.Value,
					AnnualizedRevenue6M = sixMonthMonthAnnualized == null ? 0 : (decimal) sixMonthMonthAnnualized.Value,
					AnnualizedRevenue1Y = yearAnnualized == null ? 0 : (decimal) yearAnnualized.Value,
					Revenue1Y = yearNotAnnualized == null ? 0 : (decimal)yearNotAnnualized.Value
				};

				dict[marketPlace] = onlineRevenues;
			}

			var month = GetOnlineTurnoverAnnualizedForPeriod(dict, TimePeriodEnum.Month, true);
			var threeMonth = GetOnlineTurnoverAnnualizedForPeriod(dict, TimePeriodEnum.Month3, true);
			var sixMonth = GetOnlineTurnoverAnnualizedForPeriod(dict, TimePeriodEnum.Month6, true);
			var year = GetOnlineTurnoverAnnualizedForPeriod(dict, TimePeriodEnum.Year, true);

			var revenues = new[] { month, threeMonth, sixMonth, year };
			var posRevenues = revenues.Where(x => x > 0).ToList();
			return posRevenues.Any() ? posRevenues.Max() : 0;
		}


		/// <summary>
		/// Calculate online turnover: use this formula:
		///Amazon + Max(ebay,paypal)
		///Calculate it for 1M, 3M, 6M (annualized figure) and 1Y
		///Take the minimum between these 4. (The non-zero minimum)
		/// </summary>
		public decimal GetOnlineTurnoverAnnualized(int customerId) {
			var dbHelper = new DbHelper(DB, Log);
			var mps = dbHelper.GetCustomerMarketPlaces(customerId);
			var dict = new Dictionary<MarketPlace, OnlineRevenues>();
			foreach (var marketPlace in mps) {
				dict[marketPlace] = dbHelper.GetOnlineAnnaulizedRevenueForPeriod(marketPlace.Id);
			}

			var month = GetOnlineTurnoverAnnualizedForPeriod(dict, TimePeriodEnum.Month);
			var threeMonth = GetOnlineTurnoverAnnualizedForPeriod(dict, TimePeriodEnum.Month3);
			var sixMonth = GetOnlineTurnoverAnnualizedForPeriod(dict, TimePeriodEnum.Month6);
			var year = GetOnlineTurnoverAnnualizedForPeriod(dict, TimePeriodEnum.Year);

			var revenues = new[] {month, threeMonth, sixMonth, year};
			var posRevenues = revenues.Where(x => x > 0).ToList();
			return posRevenues.Any() ? posRevenues.Min() : 0;

		}

		public decimal GetOnlineTurnoverAnnualizedForPeriod(Dictionary<MarketPlace, OnlineRevenues> dict, TimePeriodEnum timePeriod, bool useOther = false)
		{
			decimal paypal = 0;
			decimal ebay = 0;
			decimal amazon = 0;
			decimal other = 0;

			foreach (var marketPlace in dict) {
				decimal annualizedRevenue = 0;

				switch (timePeriod) {
					case TimePeriodEnum.Month:
						annualizedRevenue = marketPlace.Value.AnnualizedRevenue1M;
						break;
					case TimePeriodEnum.Month3:
						annualizedRevenue = marketPlace.Value.AnnualizedRevenue3M;
						if (annualizedRevenue == 0) {
							annualizedRevenue = marketPlace.Value.AnnualizedRevenue1M;
						}
						break;
					case TimePeriodEnum.Month6:
						annualizedRevenue = marketPlace.Value.AnnualizedRevenue6M;
						if (annualizedRevenue == 0)
						{
							annualizedRevenue = marketPlace.Value.AnnualizedRevenue3M;
						}
						if (annualizedRevenue == 0)
						{
							annualizedRevenue = marketPlace.Value.AnnualizedRevenue1M;
						}
						break;
					case TimePeriodEnum.Year:
						annualizedRevenue = marketPlace.Value.AnnualizedRevenue1Y;
						if (annualizedRevenue == 0)
						{
							annualizedRevenue = marketPlace.Value.Revenue1Y;
						}
						if (annualizedRevenue == 0)
						{
							annualizedRevenue = marketPlace.Value.AnnualizedRevenue6M;
						}
						if (annualizedRevenue == 0)
						{
							annualizedRevenue = marketPlace.Value.AnnualizedRevenue3M;
						}
						if (annualizedRevenue == 0)
						{
							annualizedRevenue = marketPlace.Value.AnnualizedRevenue1M;
						}
						break;
				}

				
				switch (marketPlace.Key.Type)
				{
					case "eBay":
						ebay += annualizedRevenue;
						break;
					case "Amazon":
						amazon += annualizedRevenue;
						break;
					case "Pay Pal":
						paypal += annualizedRevenue;
						break;
					default:
						other += annualizedRevenue;
						break;
				}
			}

			if (useOther) {
				return amazon + Math.Max(paypal, ebay) + other;
			}

			return amazon + Math.Max(paypal, ebay);
		}

		public decimal GetYodleeAnnualized(List<MarketPlace> yodlees, ASafeLog log) {
			decimal incomeAnnualized = 0;
			var dbHelper = new DbHelper(DB, log);
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
			var dbHelper = new DbHelper(DB, Log);
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

		/// <summary>
		/// calculates figures for 4 categories annual (max of annualized 1m 3m 6m and 1y), and quarter (max of 3 month not annualized): hmrc, yodlee, online,payment
		/// returns max of 4 categories for annual turnover and quarter turnover
		/// </summary>
		/// <param name="customerId">CustomerId</param>
		/// <returns>Item1=AnnualTurnover, Item2=QuarterTurnover</returns>
		public Tuple<decimal, decimal> GetTurnoverForRejection(int customerId) {
			var dbHelper = new DbHelper(DB, Log);
			var yodlees = dbHelper.GetCustomerYodlees(customerId);
			decimal yodleesAnnualized = GetYodleeAnnualized(yodlees, Log);
			decimal yodleeQuarter = yodlees.Sum(yodlee => dbHelper.GetYodleeRevenuesQuerter(yodlee.Id));

			decimal hmrcRevenueAnnualized = 0;
			decimal hmrcRevenueQuarter = 0;
			dbHelper.GetHmrcRevenuesForRejection(customerId, out hmrcRevenueAnnualized, out hmrcRevenueQuarter);

			var paymentMps = dbHelper.GetCustomerPaymentMarketPlaces(customerId);

			decimal paymentRevenueAnnualized = GetTurnoverAnnualizedForRejecetion(paymentMps);
			decimal paymentRevenueQuarter = (decimal)GetTurnoverForPeriod(paymentMps, TimePeriodEnum.Month3);

			var onlineMps = dbHelper.GetCustomerMarketPlaces(customerId);
			
			decimal onlineRevenueAnnualized = GetTurnoverAnnualizedForRejecetion(onlineMps);
			decimal onlineRevenueQuarter = (decimal)GetTurnoverForPeriod(onlineMps, TimePeriodEnum.Month3);

			decimal[] annual = new[] { hmrcRevenueAnnualized, yodleesAnnualized, onlineRevenueAnnualized, paymentRevenueAnnualized };
			decimal[] quarter = new[] { hmrcRevenueQuarter, yodleeQuarter, onlineRevenueQuarter, paymentRevenueQuarter };

			Log.Debug("Turnovers annual: hmrc: {0} yodlee: {1} online: {2} payment: {3} max: {4}", hmrcRevenueAnnualized, yodleesAnnualized, onlineRevenueAnnualized, paymentRevenueAnnualized, annual.Max());
			Log.Debug("Turnovers quarter: hmrc: {0} yodlee: {1} online: {2} payment: {3} max: {4}", hmrcRevenueQuarter, yodleeQuarter, onlineRevenueQuarter, paymentRevenueQuarter, quarter.Max());
			
			return new Tuple<decimal, decimal>(annual.Max(), quarter.Max());
		}
	}
}
