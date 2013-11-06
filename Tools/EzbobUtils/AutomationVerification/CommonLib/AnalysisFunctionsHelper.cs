using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLib
{
	public class AnalysisFunctionsHelper
	{
		public static double GetTurnoverForPeriod(List<MarketPlace> mps, TimePeriodEnum timePeriod)
		{
			double paypal = 0;
			double ebay = 0;
			double sum = 0;
			var dbHelper = new DbHelper();
			
			foreach (var marketPlace in mps)
			{
				var afs = dbHelper.GetAnalysisFunctions(marketPlace.Id);
				if (!afs.Any())
				{
					continue;
				}
				var av =
					afs.OrderByDescending(af => af.Value).LastOrDefault(af => AnalysisFunctionIncome.IncomeFunctions.Contains(af.Function) && af.TimePeriod <= timePeriod);
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


	}
}
