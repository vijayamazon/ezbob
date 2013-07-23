using System;
using System.IO;
using System.Linq;
using System.Text;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.Web.Areas.Underwriter;
using EzBob.Web.Code;
using NHibernate;
using StructureMap;
using System.IO.Compression;

namespace EzBob.Models
{
    public class StrategyHelper
    {
        private readonly CustomerRepository _customers;
        private readonly DecisionHistoryRepository _decisionHistory;
        private readonly ISession _session;
        private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;
        private MarketPlacesFacade _mpFacade;

        public StrategyHelper()
        {
            _session = ObjectFactory.GetInstance<ISession>();
            _decisionHistory = ObjectFactory.GetInstance<DecisionHistoryRepository>();
            _customers = ObjectFactory.GetInstance<CustomerRepository>();
            _caisReportsHistoryRepository = ObjectFactory.GetInstance<CaisReportsHistoryRepository>();
            _mpFacade = ObjectFactory.GetInstance<MarketPlacesFacade>();
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

            customer.DateRejected = DateTime.UtcNow;
            customer.RejectedReason = comment;

            _decisionHistory.LogAction(DecisionActions.Reject, comment, _session.Get<User>(1), customer);
        }

        public void AddApproveIntoDecisionHistory(int customerId, string comment)
        {
            var customer = _customers.Get(customerId);
            var cr = customer.LastCashRequest;

            cr.UnderwriterComment = comment;

            customer.DateApproved = DateTime.UtcNow;
            customer.ApprovedReason = comment;

            _decisionHistory.LogAction(DecisionActions.Approve, comment, _session.Get<User>(1), customer);
        }

        public int MarketplaceSeniority(int customerId)
        {
            return Convert.ToInt32((DateTime.UtcNow - _mpFacade.MarketplacesSeniority(customerId)).TotalDays);
        }

        public void SaveCAISFile(string data, string name, string foldername, int type, int ofItems, int goodUsers, int defaults)
        {
            _caisReportsHistoryRepository.AddFile(ZipString.Zip(data), name, foldername, type, ofItems, goodUsers, defaults);
        }

        public string GetCAISFileById(int id)
        {
            var file = _caisReportsHistoryRepository.Get(id);
            return file != null ? ZipString.Unzip(file.FileData) : "";
        }
    }
}