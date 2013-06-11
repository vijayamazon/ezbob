using System;
using System.Linq;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.CommonLib.TimePeriodLogic;
using NHibernate;
using StructureMap;

namespace EzBob.Models
{
    public class StrategyHelper
    {
        private readonly CustomerRepository _customers;
        private readonly DecisionHistoryRepository _decisionHistory;
        private readonly ISession _session;

        public StrategyHelper()
        {
            _session = ObjectFactory.GetInstance<ISession>();
            _decisionHistory = ObjectFactory.GetInstance<DecisionHistoryRepository>();
            _customers = ObjectFactory.GetInstance<CustomerRepository>();
        }

        public double GetAnualTurnOverByCustomer(int customerId)
        {
            var customer = _customers.Get(customerId);
            double sum = 0;
            foreach (var mp in customer.CustomerMarketPlaces)
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

        public double GetTotalSumOfOrders3M(int customerId)
        {
            var customer = _customers.Get(customerId);
            double sum = 0;
            foreach (var mp in customer.CustomerMarketPlaces)
            {
                var analisysFunction = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(mp.Id);
                var av = analisysFunction.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;
                if (av != null)
                {
                    var lastAnualTurnover = av.LastOrDefault(x => x.ParameterName == "Total Sum of Orders" && x.TimePeriod.TimePeriodType <= TimePeriodEnum.Month3);
                    sum += Convert.ToDouble(lastAnualTurnover != null ? lastAnualTurnover.Value : 0);
                }
            }
            return sum;
        }

        public void AddRejectIntoDecisionHistory(int customerId, string comment)
        {
            var customer = _customers.Get(customerId);
            var cr = customer.LastCashRequest;
            cr.UnderwriterDecision = CreditResultStatus.Rejected;
            cr.UnderwriterDecisionDate = DateTime.UtcNow;
            cr.UnderwriterComment = comment;
            _decisionHistory.LogAction(DecisionActions.Reject, comment, _session.Get<User>(1), customer);
        }

        public void AddApproveIntoDecisionHistory(int customerId, string comment)
        {
            var customer = _customers.Get(customerId);
            var cr = customer.LastCashRequest;
            cr.UnderwriterComment = comment;
            _decisionHistory.LogAction(DecisionActions.Approve, comment, _session.Get<User>(1), customer);
        }

        public int MarketplaceSeniority(int customerId)
        {
            var seniority = _customers.MarketplacesSeniority(customerId);
            var senDate = seniority != null ? seniority.Value.Date : DateTime.UtcNow;
            return Convert.ToInt32((DateTime.UtcNow - senDate).TotalDays);
        }
    }
}