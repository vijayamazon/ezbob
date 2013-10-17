namespace EzBob.Models
{
	using System;
	using System.Linq;
	using ApplicationMng.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using CommonLib.TimePeriodLogic;
	using Marketplaces;
	using Web.Code;
	using NHibernate;
	using StructureMap;
	using log4net;

	public class StrategyHelper
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(StrategyHelper));
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

		public double GetTurnoverForPeriod(int customerId, TimePeriodEnum period)
		{
			var customer = _customers.Get(customerId);
			double sum = 0;
			double payPalSum = 0;
			double ebaySum = 0;
			foreach (var mp in customer.CustomerMarketPlaces.Where(mp => !mp.Disabled &&(!mp.Marketplace.IsPaymentAccount || mp.Marketplace.Name == "Pay Pal" )))
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
		}

        public double GetAnualTurnOverByCustomer(int customerId)
        {
	        return GetTurnoverForPeriod(customerId, TimePeriodEnum.Year);
        }

		public double GetTotalSumOfOrders3M(int customerId)
		{
			return GetTurnoverForPeriod(customerId, TimePeriodEnum.Month3);
		}

		public double GetTotalSumOfOrdersForLoanOffer(int customerId)
		{
			double year = GetTurnoverForPeriod(customerId, TimePeriodEnum.Year);
			double month3 = GetTurnoverForPeriod(customerId, TimePeriodEnum.Month3);
			double month = GetTurnoverForPeriod(customerId, TimePeriodEnum.Month);

			var relevantValueForMonth = new decimal(month * 12);
			var relevantValueFor3Months = new decimal(month3 * 4);
			var relevantValueForYear = new decimal(year);
			string periodUsed;
			decimal min;
			if (relevantValueForYear <= relevantValueFor3Months && relevantValueForYear <= relevantValueForMonth)
			{
				periodUsed = "1 Year";
				min = relevantValueForYear;
			}
			else if (relevantValueFor3Months <= relevantValueForYear && relevantValueFor3Months <= relevantValueForMonth)
			{
				periodUsed = "3 Months";
				min = relevantValueFor3Months;
			}
			else
			{
				periodUsed = "1 Month";
				min = relevantValueForMonth;
			}

			log.InfoFormat("Calculated total sum of orders for loan offer. Year:{0} 3M:{1}({2} * 4) 1M:{3}({4} * 12) Calculated min:{5} Chosen period:{6}", relevantValueForYear, relevantValueFor3Months, month3, relevantValueForMonth, month, min, periodUsed);

			return (double)min;
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

		public int AutoApproveCheck(int customerId)
		{
			log.InfoFormat("Checking if auto approval should take place");
			log.InfoFormat("Decided not to auto approve");
			return 0;
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