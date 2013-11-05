using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRejection
{
	using CommonLib;

	public class AutoRejectionCalculator
    {
		public bool IsAutoRejected(int CustomerId)
		{

			return false;
		}
		/*
		public double GetTurnoverForPeriod(int customerId, TimePeriodEnum period)
		{
			var customer = _customers.Get(customerId);
			double sum = 0;
			double payPalSum = 0;
			double ebaySum = 0;
			foreach (var mp in customer.CustomerMarketPlaces.Where(mp => !mp.Disabled && (!mp.Marketplace.IsPaymentAccount || mp.Marketplace.Name == "Pay Pal")))
			{
				var analisysFunction = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(mp.Id);
				var av = analisysFunction.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;
				if (av != null)
				{
					string parameterName = mp.Marketplace.Name == "Pay Pal" ? "Total Net In Payments" : "Total Sum of Orders";
					var relevantTurnover = av.LastOrDefault(x => x.ParameterName == parameterName && x.TimePeriod.TimePeriodType <= period);

					double currentTurnover = Convert.ToDouble(relevantTurnover != null ? relevantTurnover.Value : 0);
					if (mp.Marketplace.Name == "Pay Pal")
					{
						payPalSum += currentTurnover;
					}
					else if (mp.Marketplace.Name == "eBay")
					{
						ebaySum += currentTurnover;
					}
					else
					{
						sum += currentTurnover;
					}
				}
			}
			return sum + Math.Max(payPalSum, ebaySum);
		}*/
    }
}
