namespace AutomationCalculator
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Logger;

	public class MarketPlacesHelper
	{
		public static int GetMarketPlacesSeniority(List<MarketPlace> mps)
		{
			if (mps.Any() && mps.Any(m => m.OriginationDate.HasValue))
			{
				var date = mps.Where(x => x.OriginationDate.HasValue).Max(x => x.OriginationDate.Value);
				TimeSpan ts = DateTime.Today - date;
				return (int) ts.TotalDays;
			}
			return 0;
		}

		public static double GetTurnoverForPeriod(List<MarketPlace> mps, TimePeriodEnum timePeriod, ASafeLog log)
		{
			double paypal = 0;
			double ebay = 0;
			double sum = 0;
			var dbHelper = new DbHelper(log);

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

		public static decimal GetYodleeAnnualized(List<MarketPlace> yodlees, ASafeLog log) {
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
	}
}
