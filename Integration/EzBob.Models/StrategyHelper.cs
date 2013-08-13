namespace EzBob.Models
{
	using System;
	using System.Linq;
	using ApplicationMng.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using CommonLib.TimePeriodLogic;
	using Web.Areas.Underwriter;
	using Web.Code;
	using NHibernate;
	using StructureMap;

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

		public double GetTurnoverForPeroid(int customerId, TimePeriodEnum peroid)
		{
			var customer = _customers.Get(customerId);
			double sum = 0;
			double payPalSum = 0;
			double ebaySum = 0;
			foreach (var mp in customer.CustomerMarketPlaces.Where(mp => !mp.Marketplace.IsPaymentAccount || mp.Marketplace.Name == "Pay Pal"))
			{
				var analisysFunction = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(mp.Id);
				var av = analisysFunction.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;
				if (av != null)
				{
					string parameterName = mp.Marketplace.Name == "Pay Pal" ? "Total Net In Payments" : "Total Sum of Orders";
					var relevantTurnover = av.LastOrDefault(x => x.ParameterName == parameterName && x.TimePeriod.TimePeriodType <= peroid);

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
		}

        public double GetAnualTurnOverByCustomer(int customerId)
        {
	        return GetTurnoverForPeroid(customerId, TimePeriodEnum.Year);
        }

		public double GetTotalSumOfOrders3M(int customerId)
		{
			return GetTurnoverForPeroid(customerId, TimePeriodEnum.Month3);
		}

		public double GetTotalSumOfOrdersForLoanOffer(int customerId)
		{
			double year = GetTurnoverForPeroid(customerId, TimePeriodEnum.Year);
			double month3 = GetTurnoverForPeroid(customerId, TimePeriodEnum.Month3);
			double month = GetTurnoverForPeroid(customerId, TimePeriodEnum.Month);

			return Math.Min(year, Math.Min(4 * month3, 12 * month));
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
            var customer = _customers.Get(customerId);
            return Convert.ToInt32((DateTime.UtcNow - _mpFacade.MarketplacesSeniority(customer)).TotalDays);
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