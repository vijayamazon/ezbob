using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.CommonLib.TimePeriodLogic;
using StructureMap;

namespace EzBob.Models
{
    public class StrategyHelper
    {
        private readonly CustomerRepository _customers;

        public StrategyHelper()
        {
            _customers = ObjectFactory.GetInstance<CustomerRepository>();
        }
        public double GetAnualTurnOverByCustomer(int customerId)
        {
            var customer = _customers.Get(customerId);
            double sum = 0;
            foreach (MP_CustomerMarketPlace mp in customer.CustomerMarketPlaces)
            {
                var analisysFunction = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(mp.Id);
                var av = analisysFunction.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;
                if (av != null)
                {
                    var lastAnualTurnover = av.LastOrDefault(x => x.ParameterName == "Total Sum of Orders" && x.TimePeriod.TimePeriodType <= TimePeriodEnum.Year);
                    sum += Convert.ToDouble(lastAnualTurnover!=null ? lastAnualTurnover.Value : 0);
                }
            }
            return sum;
        }
    }
}