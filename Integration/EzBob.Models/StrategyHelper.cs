namespace EzBob.Models
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml.Serialization;
	using ApplicationMng.Model;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repositories;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using CommonLib.TimePeriodLogic;
	using EzBobIntegration.Web_References.Consumer;
	using Marketplaces;
	using Web.Code;
	using NHibernate;
	using StructureMap;
	using log4net;
	using MailApi;
	using HtmlTableCreator;

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

		public int AutoApproveCheck(int customerId, int systemCalculatedAmount, int minExperianScore)
		{
			log.InfoFormat("Checking if auto approval should take place...");

			var customer = _customers.Get(customerId);
			int autoApproveMinAmount = configurationVariablesRepository.GetByNameAsInt("AutoApproveMinAmount");
			int autoApproveMaxAmount = configurationVariablesRepository.GetByNameAsInt("AutoApproveMaxAmount");
			int autoApprovedAmount = systemCalculatedAmount;

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
				if (!CheckWorstCaisStatus(customer, new List<string> {"0", "1", "2"})) // Up to 60 days
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

			log.InfoFormat("Decided to auto approve amount:{0}", autoApprovedAmount);
			return 0;
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
			decimal todayLoansAmount = loanRepository.GetAll().Where(l => l.Date.Year == today.Year && l.Date.Month == today.Month && l.Date.Day == today.Day).Sum(l => l.LoanAmount);
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

		private List<Loan> GetOutstandingLoans(int customerId)
		{
			return loanRepository.ByCustomer(customerId).Where(l => l.Status != LoanStatus.PaidOff).ToList();
		}

		private List<Loan> GetLastMonthClosedLoans(int customerId)
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
			int autoApproveMinRepaidPortion = configurationVariablesRepository.GetByNameAsInt("AutoApproveMinRepaidPortion");

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
			if (outstandingPrincipal >= autoApproveMinRepaidPortion * loanAmount)
			{
				log.InfoFormat("No auto approval: No auto approval for customers that didn't repay at least {0} of their original principal. This customer has repaid {1}", autoApproveMinRepaidPortion, outstandingPrincipal / loanAmount);
				return false;
			}

			return true;
		}

		private bool CheckWorstCaisStatus(Customer customer, List<string> allowedStatuses)
		{
			MP_ServiceLog serviceLog = serviceLogRepository.GetByCustomer(customer).OrderByDescending(sl => sl.InsertDate).FirstOrDefault();
			if (serviceLog == null)
			{
				log.InfoFormat("No auto approval: Can't find worst CAIS status in MP_ServiceLog");
				return false;
			}
			var serializer = new XmlSerializer(typeof(OutputRoot));
			using (TextReader sr = new StringReader(serviceLog.ResponseData))
			{
				var output = (OutputRoot)serializer.Deserialize(sr);

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

		public void SendFirstOfMonthStatusMail()
		{
			bool firstOfMonthStatusMailEnabled = configurationVariablesRepository.GetByNameAsBool("FirstOfMonthStatusMailEnabled");
			string firstOfMonthStatusMailCopyTo = configurationVariablesRepository.GetByName("FirstOfMonthStatusMailCopyTo").Value;
			bool firstOfMonthEnableCustomerMail = configurationVariablesRepository.GetByNameAsBool("FirstOfMonthEnableCustomerMail");

			if (!firstOfMonthStatusMailEnabled)
			{
				log.InfoFormat("The first of month status mails are disabled");
				return;
			}

			log.InfoFormat("Starting to send first of month status mails");
			foreach (Customer customer in _customers.GetAll())
			{
				List<Loan> outstandingLoans = GetOutstandingLoans(customer.Id);
				if (outstandingLoans.Count > 0)
				{
					List<Loan> closedLoans = GetLastMonthClosedLoans(customer.Id);
					log.InfoFormat("Customer {0} has {1} outstanding loans. Will send status mail to him", customer.Id, outstandingLoans.Count);
					SendStatusMailToCustomer(customer, outstandingLoans, closedLoans, firstOfMonthStatusMailCopyTo, firstOfMonthEnableCustomerMail);
				}
			}
		}

		private LoanStatusRow CreateLoanStatusRowFromTransaction(LoanTransaction loanTransaction)
		{
			var currentRow = new LoanStatusRow
			{
				PostDate = loanTransaction.PostDate,
				Description = loanTransaction.Description,
				Fees = loanTransaction.Fees.ToString("N2", CultureInfo.InvariantCulture)
			};

			var pacnetTransaction = loanTransaction as PacnetTransaction;
			if (pacnetTransaction == null)
			{
				var paypointTransaction = loanTransaction as PaypointTransaction;
				if (paypointTransaction == null)
				{
					return null;
				}
				currentRow.Type = "Payment";
				currentRow.Interest = paypointTransaction.Interest.ToString("N2", CultureInfo.InvariantCulture);
				currentRow.Principal = paypointTransaction.LoanRepayment.ToString("N2", CultureInfo.InvariantCulture);
				currentRow.Total = (paypointTransaction.LoanRepayment + paypointTransaction.Interest + loanTransaction.Fees).ToString("N2", CultureInfo.InvariantCulture);
				currentRow.Status = paypointTransaction.Status.ToString();
			}
			else
			{
				currentRow.Type = "Loan";
				currentRow.Interest = "0.00";
				currentRow.Principal = "0.00";
				currentRow.Total = "0.00";
				currentRow.Status = pacnetTransaction.Status.ToString();
			}

			return currentRow;
		}

		private void SendStatusMailToCustomer(Customer customer, List<Loan> outstandingLoans, List<Loan> closedLoans, string copyToAddress, bool shouldSendToCustomer)
		{
			log.InfoFormat("Preparing first of month mail for customer:{0}", customer.Id);
			var closedLoansSection = new StringBuilder();
			if (closedLoans.Count > 0)
			{
				closedLoansSection.Append("Loans that were closed last month:<br>");
				foreach (Loan closedLoan in closedLoans)
				{
					var rows = new List<LoanStatusRow>();
					foreach (LoanTransaction loanTransaction in closedLoan.Transactions.Where(lt => lt.Status == LoanTransactionStatus.Done))
					{
						LoanStatusRow currentRow = CreateLoanStatusRowFromTransaction(loanTransaction);
						if (currentRow != null)
						{
							rows.Add(currentRow);
						}
					}
					string tableForLoan = HtmlTableCreator.CreateHtmlTableFromClass(rows.OrderBy(p => p.PostDate));
					closedLoansSection.Append(tableForLoan).Append("<br>");
				}
				closedLoansSection.Append("<br><br>");
			}

			var outstandingLoansSection = new StringBuilder();
			if (outstandingLoans.Count > 0)
			{
				outstandingLoansSection.Append("Loans that are outstanding:<br>");
				foreach (Loan outstandingLoan in outstandingLoans)
				{
					var rows = new List<LoanStatusRow>();
					foreach (LoanTransaction loanTransaction in outstandingLoan.Transactions.Where(lt => lt.Status == LoanTransactionStatus.Done))
					{
						LoanStatusRow currentRow = CreateLoanStatusRowFromTransaction(loanTransaction);
						if (currentRow != null)
						{
							rows.Add(currentRow);
						}
					}

					foreach (var loanSchedule in outstandingLoan.Schedule.Where(ls => ls.Status != LoanScheduleStatus.Paid && ls.Status != LoanScheduleStatus.PaidEarly && ls.Status != LoanScheduleStatus.PaidOnTime))
					{
						rows.Add(new LoanStatusRow { Type = "Schedule", PostDate = loanSchedule.Date, Description = string.Empty, Fees = loanSchedule.Fees.ToString("N2", CultureInfo.InvariantCulture), Interest = loanSchedule.Interest.ToString("N2", CultureInfo.InvariantCulture), Principal = loanSchedule.LoanRepayment.ToString("N2", CultureInfo.InvariantCulture), Status = loanSchedule.Status.ToString(), Total = (loanSchedule.LoanRepayment + loanSchedule.Interest + loanSchedule.Fees).ToString("N2", CultureInfo.InvariantCulture) });
					}

					string tableForLoan = HtmlTableCreator.CreateHtmlTableFromClass(rows.OrderBy(p => p.PostDate));
					outstandingLoansSection.Append(tableForLoan).Append("<br>");
				}
			}

			if (shouldSendToCustomer)
			{
				SendStatusMail(customer.Name, customer.PersonalInfo.FirstName, closedLoansSection.ToString(), outstandingLoansSection.ToString());
			}
			if (!string.IsNullOrEmpty(copyToAddress))
			{
				SendStatusMail(copyToAddress, customer.PersonalInfo.FirstName, closedLoansSection.ToString(), outstandingLoansSection.ToString());
			}
		}

		private void SendStatusMail(string toAddress, string firstName, string closedLoansSection, string outstandingLoansSection)
		{
			string firstOfMonthStatusMailMandrillTemplateName = configurationVariablesRepository.GetByName("FirstOfMonthStatusMailMandrillTemplateName").Value;
			var mail = ObjectFactory.GetInstance<IMail>();

			var vars = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"ClosedLoansSection", closedLoansSection},
					{"OutstandingLoansSection", outstandingLoansSection} 
				};

			var result = mail.Send(vars, toAddress, firstOfMonthStatusMailMandrillTemplateName);
			if (result == "OK")
			{
				log.InfoFormat("Sent mail - {0}", firstOfMonthStatusMailMandrillTemplateName);
			}
			else
			{
				log.ErrorFormat("Failed sending alert mail - {0}. Result:{1}", result, firstOfMonthStatusMailMandrillTemplateName);
			}
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
	}

	public class LoanStatusRow
	{
		public string Type { get; set; }
		public DateTime PostDate { get; set; }
		public string Principal { get; set; }
		public string Interest { get; set; }
		public string Fees { get; set; }
		public string Total { get; set; }
		public string Status { get; set; }
		public string Description { get; set; }
	}
}