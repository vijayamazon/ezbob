namespace EzBob.Models
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Xml.Serialization;
	using ApplicationMng.Model;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using CommonLib.TimePeriodLogic;
	using EzBobIntegration.Web_References.Consumer;
	using Marketplaces;
	using Web.Code;
	using NHibernate;
	using StructureMap;
	using ZooplaLib;
	using log4net;
	using MailApi;

	public class StrategyHelper
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(StrategyHelper));
		private readonly CustomerRepository _customers;
		private readonly DecisionHistoryRepository _decisionHistory;
		private readonly ISession _session;
		private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;
		private readonly MarketPlacesFacade _mpFacade;
		private readonly PacNetBalanceRepository pacNetBalanceRepository;
		private readonly LoanRepository loanRepository;
		private readonly CashRequestsRepository cashRequestsRepository;
		private readonly ExperianDefaultAccountRepository experianDefaultAccountRepository;
		private readonly LoanScheduleTransactionRepository loanScheduleTransactionRepository;
		private readonly ConfigurationVariablesRepository configurationVariablesRepository;
		private readonly ServiceLogRepository serviceLogRepository;
		public StrategyHelper()
		{
			_session = ObjectFactory.GetInstance<ISession>();
			_decisionHistory = ObjectFactory.GetInstance<DecisionHistoryRepository>();
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
			_caisReportsHistoryRepository = ObjectFactory.GetInstance<CaisReportsHistoryRepository>();
			_mpFacade = ObjectFactory.GetInstance<MarketPlacesFacade>();
			pacNetBalanceRepository = ObjectFactory.GetInstance<PacNetBalanceRepository>();
			loanRepository = ObjectFactory.GetInstance<LoanRepository>();
			cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
			experianDefaultAccountRepository = ObjectFactory.GetInstance<ExperianDefaultAccountRepository>();
			loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();
			configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();
			serviceLogRepository = ObjectFactory.GetInstance<ServiceLogRepository>();
		}

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

		public int AutoApproveCheck(int customerId, int systemCalculatedAmount, int minExperianScore)
		{
			log.InfoFormat("Checking if auto approval should take place...");
			var customer = _customers.Get(customerId);
			int autoApproveMinAmount = configurationVariablesRepository.GetByNameAsInt("AutoApproveMinAmount");
			int autoApproveMaxAmount = configurationVariablesRepository.GetByNameAsInt("AutoApproveMaxAmount");
			int autoApprovedAmount = systemCalculatedAmount;

			try
			{
				if (!CheckAMLResult(customer) ||
					!CheckCustomerType(customer) ||
					!CheckCustomerStatus(customer) ||
					!CheckExperianScore(minExperianScore) ||
					!CheckAge(customer) ||
					!CheckTurnovers(customerId) ||
					!CheckSeniority(customerId) ||
					!CheckOutstandingOffers() ||
					!CheckTodaysLoans() ||
					!CheckTodaysApprovals() ||
					!CheckDefaultAccounts(customerId))
				{
					return 0;
				}

				if (!loanRepository.ByCustomer(customerId).Any())
				{
					if (!CheckWorstCaisStatus(customer, new List<string> { "0", "1", "2" })) // Up to 60 days
					{
						return 0;
					}
				}
				else
				{
					if (!CheckWorstCaisStatus(customer, new List<string> { "0", "1", "2", "3" }) || // Up to 90 days
						!CheckRollovers(customerId) ||
						!CheckLateDays(customerId) ||
						!CheckOutstandingLoans(customerId))
					{
						return 0;
					}

					// Reduce the system calculated amount by the already open amount
					List<Loan> outstandingLoans = GetOutstandingLoans(customerId);
					decimal outstandingPrincipal = outstandingLoans.Sum(loan => loan.Principal);
					autoApprovedAmount -= (int)outstandingPrincipal;
				}

				if (autoApprovedAmount < autoApproveMinAmount || autoApprovedAmount > autoApproveMaxAmount)
				{
					log.InfoFormat("No auto approval: System calculated amount is not between {0}-{1} but is {2}", autoApproveMinAmount, autoApproveMaxAmount, autoApprovedAmount);
					return 0;
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("No auto approval: Exception while checking auto approval conditions:{0}", ex);
				return 0;
			}

			log.InfoFormat("Decided to auto approve amount:{0}", autoApprovedAmount);
			return autoApprovedAmount;
		}

		private bool CheckDefaultAccounts(int customerId)
		{
			if (experianDefaultAccountRepository.GetAll().Any(entry => entry.Customer.Id == customerId))
			{
				log.InfoFormat("No auto approval: No auto approval for customer with default accounts");
				return false;
			}

			return true;
		}

		private bool CheckAMLResult(Customer customer)
		{
			if (customer.AMLResult != "Passed")
			{
				log.InfoFormat("No auto approval: AML is not passed");
				return false;
			}

			return true;
		}

		private bool CheckCustomerType(Customer customer)
		{
			if (customer.IsOffline)
			{
				log.InfoFormat("No auto approval: Offline customer");
				return false;
			}

			return true;
		}

		private bool CheckCustomerStatus(Customer customer)
		{
			if (!customer.CollectionStatus.CurrentStatus.IsEnabled)
			{
				log.InfoFormat("No auto approval: Only 'enabled' statuses are allowed for auto approval. Customer status is:{0}", customer.CollectionStatus.CurrentStatus.Name);
				return false;
			}

			return true;
		}

		private bool CheckExperianScore(int minExperianScore)
		{
			int autoApproveExperianScoreThreshold = configurationVariablesRepository.GetByNameAsInt("AutoApproveExperianScoreThreshold");
			if (minExperianScore < autoApproveExperianScoreThreshold)
			{
				log.InfoFormat("No auto approval: Minimal score threshold is: {0}. Customer minimal score (considering directors) is:{1}", autoApproveExperianScoreThreshold, minExperianScore);
				return false;
			}

			return true;
		}

		private bool CheckAge(Customer customer)
		{
			int? customerAge = null;
			if (customer.PersonalInfo.DateOfBirth != null)
			{
				DateTime now = DateTime.UtcNow;
				customerAge = now.Year - customer.PersonalInfo.DateOfBirth.Value.Year;
				if (now < customer.PersonalInfo.DateOfBirth.Value.AddYears(customerAge.Value)) customerAge--;
			}

			int autoApproveCustomerMinAge = configurationVariablesRepository.GetByNameAsInt("AutoApproveCustomerMinAge");
			int autoApproveCustomerMaxAge = configurationVariablesRepository.GetByNameAsInt("AutoApproveCustomerMaxAge");

			if (customerAge == null || customerAge < autoApproveCustomerMinAge || customerAge > autoApproveCustomerMaxAge)
			{
				log.InfoFormat("No auto approval: Customer age should be between {0}-{1} for auto approval. Customer age is {2}", autoApproveCustomerMinAge, autoApproveCustomerMaxAge, customerAge == null ? "Unknown" : customerAge.Value.ToString(CultureInfo.InvariantCulture));
				return false;
			}

			return true;
		}

		private bool CheckTurnovers(int customerId)
		{
			int autoApproveMinTurnover1M = configurationVariablesRepository.GetByNameAsInt("AutoApproveMinTurnover1M");
			int autoApproveMinTurnover3M = configurationVariablesRepository.GetByNameAsInt("AutoApproveMinTurnover3M");
			int autoApproveMinTurnover1Y = configurationVariablesRepository.GetByNameAsInt("AutoApproveMinTurnover1Y");

			int turnover1M = (int)GetTurnoverForPeriod(customerId, TimePeriodEnum.Month);
			if (turnover1M < autoApproveMinTurnover1M)
			{
				log.InfoFormat("No auto approval: Minimal 1 month turnover for auto approval is: {0}. Customer 1 month turnover is:{1}", autoApproveMinTurnover1M, turnover1M);
				return false;
			}

			int turnover3M = (int)GetTurnoverForPeriod(customerId, TimePeriodEnum.Month3);
			if (turnover3M < autoApproveMinTurnover3M)
			{
				log.InfoFormat("No auto approval: Minimal 3 months turnover for auto approval is: {0}. Customer 3 months turnover is:{1}", autoApproveMinTurnover3M, turnover3M);
				return false;
			}

			int turnover1Y = (int)GetTurnoverForPeriod(customerId, TimePeriodEnum.Year);
			if (turnover1Y < autoApproveMinTurnover1Y)
			{
				log.InfoFormat("No auto approval: Minimal 1 year turnover for auto approval is: {0}. Customer 1 year turnover is:{1}", autoApproveMinTurnover1Y, turnover1Y);
				return false;
			}

			return true;
		}

		private bool CheckSeniority(int customerId)
		{
			int autoApproveMinMpSeniorityDays = configurationVariablesRepository.GetByNameAsInt("AutoApproveMinMPSeniorityDays");

			int marketplaceSeniorityInDays = MarketplaceSeniority(customerId);
			if (marketplaceSeniorityInDays < autoApproveMinMpSeniorityDays)
			{
				log.InfoFormat("No auto approval: Minimal marketplace seniority for auto approval is: {0}. Customer marketplace seniority is:{1}", autoApproveMinMpSeniorityDays, marketplaceSeniorityInDays);
				return false;
			}

			return true;
		}

		private bool CheckOutstandingOffers()
		{
			int autoApproveMaxOutstandingOffers = configurationVariablesRepository.GetByNameAsInt("AutoApproveMaxOutstandingOffers");
			decimal outstandingOffers = pacNetBalanceRepository.GetBalance().ReservedAmount;
			if (outstandingOffers >= autoApproveMaxOutstandingOffers)
			{
				log.InfoFormat("No auto approval: Maximal allowed outstanding offers for auto approval is: {0}. Outstanding offers amount is:{1}", autoApproveMaxOutstandingOffers, outstandingOffers);
				return false;
			}

			return true;
		}

		private bool CheckTodaysLoans()
		{
			int autoApproveMaxTodayLoans = configurationVariablesRepository.GetByNameAsInt("AutoApproveMaxTodayLoans");
			DateTime today = DateTime.UtcNow;
			var todayLoans = loanRepository.GetAll().Where(l => l.Date.Year == today.Year && l.Date.Month == today.Month && l.Date.Day == today.Day);
			decimal todayLoansAmount = 0;
			if (todayLoans.Any())
			{
				todayLoansAmount = todayLoans.Sum(l => l.LoanAmount);
			}
			if (todayLoansAmount >= autoApproveMaxTodayLoans)
			{
				log.InfoFormat("No auto approval: Maximal allowed today's loans for auto approval is: {0}. Today's loan amount is:{1}", autoApproveMaxTodayLoans, todayLoansAmount);
				return false;
			}

			return true;
		}

		private bool CheckTodaysApprovals()
		{
			int autoApproveMaxDailyApprovals = configurationVariablesRepository.GetByNameAsInt("AutoApproveMaxDailyApprovals");
			DateTime today = DateTime.UtcNow;
			int numOfApprovalsToday = cashRequestsRepository.GetAll().Count(cr => cr.CreationDate.HasValue && cr.CreationDate.Value.Year == today.Year && cr.CreationDate.Value.Month == today.Month && cr.CreationDate.Value.Day == today.Day && cr.UnderwriterComment == "Auto Approval");
			if (numOfApprovalsToday >= autoApproveMaxDailyApprovals)
			{
				log.InfoFormat("No auto approval: Maximal allowed auto approvals per day is: {0}.", autoApproveMaxDailyApprovals);
				return false;
			}

			return true;
		}

		private bool CheckRollovers(int customerId)
		{
			if (loanRepository.ByCustomer(customerId).Any(l => l.Schedule.Any(s => s.Rollovers.Any())))
			{
				log.InfoFormat("No auto approval: No auto approval for customers with rollovers");
				return false;
			}

			return true;
		}

		private bool CheckLateDays(int customerId)
		{
			int autoApproveMaxAllowedDaysLate = configurationVariablesRepository.GetByNameAsInt("AutoApproveMaxAllowedDaysLate");
			List<int> customerLoanIds = loanRepository.ByCustomer(customerId).Select(d => d.Id).ToList();
			foreach (int loanId in customerLoanIds)
			{
				int innerLoanId = loanId;
				var backfilledMapping = loanScheduleTransactionRepository.GetAll().Where(x => x.Loan.Id == innerLoanId);
				if (!backfilledMapping.Any())
				{
					log.InfoFormat("No auto approval: Can't verify there were no late payments, can't find relevant entries in LoanScheduleTransaction for loan: {0}", innerLoanId);
					return false;
				}

				foreach (var paymentMapping in backfilledMapping)
				{
					var scheduleDate = new DateTime(paymentMapping.Schedule.Date.Year, paymentMapping.Schedule.Date.Month, paymentMapping.Schedule.Date.Day);
					var transactionDate = new DateTime(paymentMapping.Transaction.PostDate.Year, paymentMapping.Transaction.PostDate.Month, paymentMapping.Transaction.PostDate.Day);
					if (transactionDate.Subtract(scheduleDate).TotalDays > autoApproveMaxAllowedDaysLate)
					{
						log.InfoFormat("No auto approval: No auto approvals if there were lates over {0} days. This customer was {1} days late for loan: {2} schedule: {3} transaction: {4}", autoApproveMaxAllowedDaysLate, transactionDate.Subtract(scheduleDate).TotalDays, innerLoanId, paymentMapping.Schedule.Id, paymentMapping.Transaction.Id);
						return false;
					}
				}
			}

			return true;
		}

		internal List<Loan> GetOutstandingLoans(int customerId)
		{
			return loanRepository.ByCustomer(customerId).Where(l => l.Status != LoanStatus.PaidOff).ToList();
		}

		internal List<Loan> GetLastMonthClosedLoans(int customerId)
		{
			DateTime now = DateTime.UtcNow;
			DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
			DateTime endOfLastMonth = startOfMonth.Subtract(TimeSpan.FromMilliseconds(1));
			DateTime startOfLastMonth = new DateTime(endOfLastMonth.Year, endOfLastMonth.Month, 1);
			return loanRepository.ByCustomer(customerId).Where(l => l.Status == LoanStatus.PaidOff && l.DateClosed.HasValue && l.DateClosed >= startOfLastMonth && l.DateClosed <= endOfLastMonth).ToList();
		}

		public int GetOutstandingLoansNum(int customerId)
		{
			return GetOutstandingLoans(customerId).Count;
		}

		private bool CheckOutstandingLoans(int customerId)
		{
			int autoApproveMaxNumOfOutstandingLoans = configurationVariablesRepository.GetByNameAsInt("AutoApproveMaxNumOfOutstandingLoans");
			decimal autoApproveMinRepaidPortion = configurationVariablesRepository.GetByNameAsDecimal("AutoApproveMinRepaidPortion");

			List<Loan> outstandingLoans = GetOutstandingLoans(customerId);
			if (outstandingLoans.Count > autoApproveMaxNumOfOutstandingLoans)
			{
				log.InfoFormat("No auto approval: No auto approval for customers with more than {0} outstanding loans. This customer has {1} outstanding loans.", autoApproveMaxNumOfOutstandingLoans, outstandingLoans.Count);
				return false;
			}

			decimal loanAmount = 0, outstandingPrincipal = 0;
			foreach (var loan in outstandingLoans)
			{
				loanAmount += loan.LoanAmount;
				outstandingPrincipal += loan.Principal;

			}
			if (outstandingPrincipal != 0 && outstandingPrincipal >= autoApproveMinRepaidPortion * loanAmount)
			{
				log.InfoFormat("No auto approval: No auto approval for customers that didn't repay at least {0} of their original principal. This customer has repaid {1}", autoApproveMinRepaidPortion, loanAmount == 0 ? 0 : outstandingPrincipal / loanAmount);
				return false;
			}

			return true;
		}

		private bool CheckWorstCaisStatus(Customer customer, List<string> allowedStatuses)
		{
			log.InfoFormat("checking worst cais status");

			MP_ServiceLog serviceLog = serviceLogRepository.GetByCustomer(customer).Where(sl => sl.ServiceType == "Consumer Request").OrderByDescending(sl => sl.InsertDate).FirstOrDefault();
			if (serviceLog == null)
			{
				log.InfoFormat("No auto approval: Can't find worst CAIS status in MP_ServiceLog");
				return false;
			}
			var serializer = new XmlSerializer(typeof(OutputRoot));
			using (TextReader sr = new StringReader(serviceLog.ResponseData))
			{
				var output = (OutputRoot)serializer.Deserialize(sr);

				if (output == null || output.Output == null || output.Output.FullConsumerData == null || output.Output.FullConsumerData.ConsumerData == null || output.Output.FullConsumerData.ConsumerData.CAIS == null)
				{
					log.InfoFormat("No auto approval: Can't find worst CAIS status in deserialized response");
					return false;
				}

				foreach (var caisPart in output.Output.FullConsumerData.ConsumerData.CAIS)
				{
					foreach (var caisDetailsPart in caisPart.CAISDetails)
					{
						if (!allowedStatuses.Contains(caisDetailsPart.WorstStatus))
						{
							log.InfoFormat("No auto approval: Worst CAIS status is {0}. Allowed CAIS statuses are: {1}", caisDetailsPart.WorstStatus, allowedStatuses.Aggregate((i, j) => i + "," + j));
							return false;
						}
					}
				}
			}

			return true;
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

		public void NotifyAutoApproveSilentMode(int customerId, int autoApproveAmount, string autoApproveSilentTemplateName, string autoApproveSilentToAddress)
		{
			try
			{
				log.InfoFormat("Sending silent auto approval mail for: customerId={0} autoApproveAmount={1} autoApproveSilentTemplateName={2} autoApproveSilentToAddress={3}", customerId, autoApproveAmount, autoApproveSilentTemplateName, autoApproveSilentToAddress);
				var mail = ObjectFactory.GetInstance<IMail>();
				var vars = new Dictionary<string, string>
				{
					{"customerId", customerId.ToString(CultureInfo.InvariantCulture)},
					{"autoApproveAmount", autoApproveAmount.ToString(CultureInfo.InvariantCulture)}
				};

				var result = mail.Send(vars, autoApproveSilentToAddress, autoApproveSilentTemplateName);
				if (result == "OK")
				{
					log.InfoFormat("Sent mail - silent auto approval");
				}
				else
				{
					log.ErrorFormat("Failed sending alert mail - silent auto approval. Result:{0}", result);
				}
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed sending alert mail - silent auto approval. Exception:{0}", e);
			}

		}

		public void GetZooplaData(int customerId, bool reCheck = false)
		{
			var customer = _customers.Get(customerId);
			var customerAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault();
			if (customerAddress != null)
			{
				if (customerAddress.Zoopla.Any() || reCheck)
				{
					var zooplaApi = new ZooplaApi();
					try
					{
						var areaValueGraphs = zooplaApi.GetAreaValueGraphs(customerAddress.Postcode);
						var averageSoldPrices = zooplaApi.GetAverageSoldPrices(customerAddress.Postcode);
						var zooplaEstimate = zooplaApi.GetZooplaEstimate(customerAddress.ZooplaAddress);
						customerAddress.Zoopla.Add(new Zoopla
						{
							AreaName = averageSoldPrices.AreaName,
							AverageSoldPrice1Year = averageSoldPrices.AverageSoldPrice1Year,
							AverageSoldPrice3Year = averageSoldPrices.AverageSoldPrice3Year,
							AverageSoldPrice5Year = averageSoldPrices.AverageSoldPrice5Year,
							AverageSoldPrice7Year = averageSoldPrices.AverageSoldPrice7Year,
							NumerOfSales1Year = averageSoldPrices.NumerOfSales1Year,
							NumerOfSales3Year = averageSoldPrices.NumerOfSales3Year,
							NumerOfSales5Year = averageSoldPrices.NumerOfSales5Year,
							NumerOfSales7Year = averageSoldPrices.NumerOfSales7Year,
							TurnOver = averageSoldPrices.TurnOver,
							PricesUrl = averageSoldPrices.PricesUrl,
							AverageValuesGraphUrl = areaValueGraphs.AverageValuesGraphUrl,
							HomeValuesGraphUrl = areaValueGraphs.HomeValuesGraphUrl,
							ValueRangesGraphUrl = areaValueGraphs.ValueRangesGraphUrl,
							ValueTrendGraphUrl = areaValueGraphs.ValueTrendGraphUrl,
							CustomerAddress = customerAddress,
							ZooplaEstimate = zooplaEstimate,
							UpdateDate = DateTime.UtcNow
						});

						_session.Flush();
					}
					catch (Exception arg)
					{
						log.ErrorFormat("Zoopla error {0}", arg);
					}
				}
			}
		}
	}
}