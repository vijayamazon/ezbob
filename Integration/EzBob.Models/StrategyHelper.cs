namespace EzBob.Models
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using ApplicationMng.Repository;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using CommonLib.TimePeriodLogic;
	using Ezbob.Backend.Models;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using LandRegistryLib;
	using Marketplaces;
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
		private readonly LoanRepository loanRepository;
		private readonly CashRequestsRepository cashRequestsRepository;
		private readonly ExperianDefaultAccountRepository experianDefaultAccountRepository;
		private readonly LoanScheduleTransactionRepository loanScheduleTransactionRepository;
		private readonly CustomerMarketPlaceRepository _marketPlaceRepository;
		private readonly CustomerAddressRepository customerAddressRepository;
		private readonly LandRegistryRepository landRegistryRepository;

		public StrategyHelper()
		{
			_session = ObjectFactory.GetInstance<ISession>();
			_decisionHistory = ObjectFactory.GetInstance<DecisionHistoryRepository>();
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
			_caisReportsHistoryRepository = ObjectFactory.GetInstance<CaisReportsHistoryRepository>();
			_mpFacade = ObjectFactory.GetInstance<MarketPlacesFacade>();
			loanRepository = ObjectFactory.GetInstance<LoanRepository>();
			cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
			experianDefaultAccountRepository = ObjectFactory.GetInstance<ExperianDefaultAccountRepository>();
			loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();
			_marketPlaceRepository = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>();
			customerAddressRepository = ObjectFactory.GetInstance<CustomerAddressRepository>();
			landRegistryRepository = ObjectFactory.GetInstance<LandRegistryRepository>();
		}

		public double GetAnualTurnOverByCustomer(int customerId)
		{
			var analysisVals = GetAnalysisValsForCustomer(customerId);
			return GetTurnoverForPeriod(analysisVals, TimePeriodEnum.Year);
		}

		private double Get3MTurnoverFromValues(List<IAnalysisDataParameterInfo> av, string mpName)
		{
			string annualizedParameterName;
			switch (mpName)
			{
				case "Pay Pal":
					annualizedParameterName = "Total Net In Payments";
					break;
				case "Yodlee":
					annualizedParameterName = "Total Income";
					break;
				default:
					annualizedParameterName = "Total Sum of Orders";
					break;
			}

			IAnalysisDataParameterInfo relevantTurnover = av.LastOrDefault(x => x.ParameterName == annualizedParameterName && x.TimePeriod.TimePeriodType == TimePeriodEnum.Month3) ??
														  av.LastOrDefault(x => x.ParameterName == annualizedParameterName && x.TimePeriod.TimePeriodType == TimePeriodEnum.Month);

			return Convert.ToDouble(relevantTurnover != null ? relevantTurnover.Value : 0);
		}


		private double GetAnnualizedTurnoverFromValues(List<IAnalysisDataParameterInfo> av, string mpName)
		{
			string annualizedParameterName;
			switch (mpName)
			{
				case "Pay Pal":
					annualizedParameterName = "Total Net In Payments Annualized";
					break;
				case "Yodlee":
					annualizedParameterName = "Total Income Annualized";
					break;
				default:
					annualizedParameterName = "Total Sum of Orders Annualized";
					break;
			}

			IAnalysisDataParameterInfo relevantTurnover = ((av.LastOrDefault(x => x.ParameterName == annualizedParameterName && x.TimePeriod.TimePeriodType == TimePeriodEnum.Year) ??
															av.LastOrDefault(x => x.ParameterName == annualizedParameterName && x.TimePeriod.TimePeriodType == TimePeriodEnum.Month6)) ??
														   av.LastOrDefault(x => x.ParameterName == annualizedParameterName && x.TimePeriod.TimePeriodType == TimePeriodEnum.Month3)) ??
														  av.LastOrDefault(x => x.ParameterName == annualizedParameterName && x.TimePeriod.TimePeriodType == TimePeriodEnum.Month);

			return Convert.ToDouble(relevantTurnover != null ? relevantTurnover.Value : 0);
		}

		private double GetYodleeForRejection(int customerId, Func<List<IAnalysisDataParameterInfo>, string, double> func)
		{
			var analysisVals = GetAllAnalysisValsForCustomer(customerId);
			bool hasYodlee = false;
			double bankSum = 0;
			foreach (var mp in analysisVals)
			{
				List<IAnalysisDataParameterInfo> av = mp.Value;
				if (av != null)
				{
					if (mp.Key.Marketplace.Name == "Yodlee")
					{
						hasYodlee = true;
						bankSum += func(av, mp.Key.Marketplace.Name);
					}
				}
			}
			return hasYodlee ? bankSum : -1;
		}

		public double GetTurnoverForRejection(int customerId, Func<List<IAnalysisDataParameterInfo>, string, double> func)
		{
			var analysisVals = GetAllAnalysisValsForCustomer(customerId);

			double bankSum = 0;
			double vatSum = 0;
			double accountingSum = 0;
			double shopsSum = 0;
			double payPalSum = 0;
			double ebaySum = 0;
			foreach (var mp in analysisVals)
			{
				List<IAnalysisDataParameterInfo> av = mp.Value;
				if (av != null)
				{
					double currentTurnover = func(av, mp.Key.Marketplace.Name);

					if (mp.Key.Marketplace.Name == "Pay Pal")
					{
						payPalSum += currentTurnover;
					}
					else if (mp.Key.Marketplace.Name == "eBay")
					{
						ebaySum += currentTurnover;
					}
					else if (mp.Key.Marketplace.Name == "HMRC")
					{
						vatSum += currentTurnover;
					}
					else if (mp.Key.Marketplace.Name == "Yodlee")
					{
						bankSum += currentTurnover;
					}
					else if (mp.Key.Marketplace.IsPaymentAccount)
					{
						accountingSum += currentTurnover;
					}
					else
					{
						shopsSum += currentTurnover;
					}
				}
			}

			return Math.Max(shopsSum + Math.Max(payPalSum, ebaySum), Math.Max(accountingSum, /*Math.Max(*/bankSum/*, vatSum)*/)); // The vat is commented out until the hmrc aggregated values will be populated properly
		}

		public double GetTotalSumOfOrders3M(int customerId)
		{
			var analysisVals = GetAnalysisValsForCustomer(customerId);
			return GetTurnoverForPeriod(analysisVals, TimePeriodEnum.Month3);
		}

		public Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>> GetAnalysisValsForCustomer(
			int customerId)
		{
			var mps = _marketPlaceRepository.GetAllByCustomer(customerId);
			var mpAnalysis = new Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>>();
			foreach (var mp in mps.Where(
						mp => !mp.Disabled && (!mp.Marketplace.IsPaymentAccount || mp.Marketplace.Name == "Pay Pal")))
			{
				var analisysFunction = mp.GetRetrieveDataHelper().GetAnalysisValuesByCustomerMarketPlace(mp.Id);
				var av = analisysFunction.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;
				if (av != null)
				{
					mpAnalysis.Add(mp, av);
				}
			}

			return mpAnalysis;
		}

		public Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>> GetAllAnalysisValsForCustomer(int customerId)
		{
			var mps = _marketPlaceRepository.GetAllByCustomer(customerId);
			var mpAnalysis = new Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>>();
			foreach (var mp in mps.Where(mp => !mp.Disabled))
			{
				var analisysFunction = mp.GetRetrieveDataHelper().GetAnalysisValuesByCustomerMarketPlace(mp.Id);
				var av = analisysFunction.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;
				if (av != null)
				{
					mpAnalysis.Add(mp, av);
				}
			}

			return mpAnalysis;
		}

		public double GetTurnoverForPeriod(Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>> analysisVals, TimePeriodEnum period)
		{
			double sum = 0;
			double payPalSum = 0;
			double ebaySum = 0;
			foreach (var mp in analysisVals)
			{
				var av = mp.Value;
				if (av != null)
				{
					string parameterName = mp.Key.Marketplace.Name == "Pay Pal" ? "Total Net In Payments" : "Total Sum of Orders";
					var relevantTurnover = av.LastOrDefault(x => x.ParameterName == parameterName && x.TimePeriod.TimePeriodType <= period);

					double currentTurnover = Convert.ToDouble(relevantTurnover != null ? relevantTurnover.Value : 0);
					if (mp.Key.Marketplace.Name == "Pay Pal")
					{
						payPalSum += currentTurnover;
					}
					else if (mp.Key.Marketplace.Name == "eBay")
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

		public double GetTotalSumOfOrdersForLoanOffer(int customerId)
		{
			var analysisVals = GetAnalysisValsForCustomer(customerId);

			double year = GetAnnualizedTurnoverForPeriod(analysisVals, TimePeriodEnum.Year);
			double month6 = GetAnnualizedTurnoverForPeriod(analysisVals, TimePeriodEnum.Month6);
			double month3 = GetAnnualizedTurnoverForPeriod(analysisVals, TimePeriodEnum.Month3);
			double month = GetAnnualizedTurnoverForPeriod(analysisVals, TimePeriodEnum.Month);

			double min = Math.Min(year, Math.Min(month6, Math.Min(month3, month)));
			log.InfoFormat("Calculated annualized turnover. Year:{0} 6Months:{1} 3Months:{2} Month:{3}. Using min:{4}", year, month6, month3, month, min);
			return min;
		}

		public double GetAnnualizedTurnoverForPeriod(Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>> mpAnalysis, TimePeriodEnum period)
		{
			double sum = 0;
			double payPalSum = 0;
			double ebaySum = 0;
			foreach (var mp in mpAnalysis)
			{
				var av = mp.Value;
				if (av != null)
				{
					string annualizedParameterName = mp.Key.Marketplace.Name == "Pay Pal" ? "Total Net In Payments Annualized" : "Total Sum of Orders Annualized";
					IAnalysisDataParameterInfo relevantTurnover;
					if (period == TimePeriodEnum.Month || period == TimePeriodEnum.Month3 || period == TimePeriodEnum.Month6)
					{
						relevantTurnover = av.LastOrDefault(x => x.ParameterName == annualizedParameterName && x.TimePeriod.TimePeriodType <= period);
					}
					else
					{
						string parameterName = mp.Key.Marketplace.Name == "Pay Pal" ? "Total Net In Payments" : "Total Sum of Orders";
						relevantTurnover = av.LastOrDefault(x => x.ParameterName == parameterName && x.TimePeriod.TimePeriodType == period);
						if (relevantTurnover == null)
						{
							relevantTurnover = av.LastOrDefault(x => x.ParameterName == annualizedParameterName && x.TimePeriod.TimePeriodType <= period);
						}
					}

					double currentTurnover = Convert.ToDouble(relevantTurnover != null ? relevantTurnover.Value : 0);
					if (mp.Key.Marketplace.Name == "Pay Pal")
					{
						payPalSum += currentTurnover;
					}
					else if (mp.Key.Marketplace.Name == "eBay")
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

		public int MarketplaceSeniority(Customer customer)
		{
			return Convert.ToInt32((DateTime.UtcNow - _mpFacade.MarketplacesSeniority(customer)).TotalDays);
		}

		public int AutoApproveCheck(int customerId, int systemCalculatedAmount, int minExperianScore, decimal outstandingOffers, List<string> consumerCaisDetailWorstStatuses)
		{
			log.InfoFormat("Checking if auto approval should take place...");
			var customer = _customers.Get(customerId);
			int autoApproveMinAmount = CurrentValues.Instance.AutoApproveMinAmount;
			int autoApproveMaxAmount = CurrentValues.Instance.AutoApproveMaxAmount;
			int autoApprovedAmount = systemCalculatedAmount;

			try
			{
				if (!CheckAMLResult(customer) ||
					!CheckCustomerType(customer) ||
					!CheckCustomerStatus(customer) ||
					!CheckExperianScore(minExperianScore) ||
					!CheckAge(customer) ||
					!CheckTurnovers(customerId) ||
					!CheckSeniority(customer) ||
					!CheckOutstandingOffers(outstandingOffers) ||
					!CheckTodaysLoans() ||
					!CheckTodaysApprovals() ||
					!CheckDefaultAccounts(customerId))
				{
					return 0;
				}

				if (!loanRepository.ByCustomer(customerId).Any())
				{
					if (!CheckWorstCaisStatus(new List<string> { "0", "1", "2" }, consumerCaisDetailWorstStatuses)) // Up to 60 days
					{
						return 0;
					}
				}
				else
				{
					if (!CheckWorstCaisStatus(new List<string> { "0", "1", "2", "3" }, consumerCaisDetailWorstStatuses) || // Up to 90 days
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

			log.InfoFormat("Calculated auto approve amount:{0}", autoApprovedAmount);
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
			if (customer.IsOffline.HasValue && customer.IsOffline.Value)
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
			int autoApproveExperianScoreThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold;
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

			int autoApproveCustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge;
			int autoApproveCustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge;

			if (customerAge == null || customerAge < autoApproveCustomerMinAge || customerAge > autoApproveCustomerMaxAge)
			{
				log.InfoFormat("No auto approval: Customer age should be between {0}-{1} for auto approval. Customer age is {2}", autoApproveCustomerMinAge, autoApproveCustomerMaxAge, customerAge == null ? "Unknown" : customerAge.Value.ToString(CultureInfo.InvariantCulture));
				return false;
			}

			return true;
		}

		private bool CheckTurnovers(int customerId)
		{
			int autoApproveMinTurnover1M = CurrentValues.Instance.AutoApproveMinTurnover1M;
			int autoApproveMinTurnover3M = CurrentValues.Instance.AutoApproveMinTurnover3M;
			int autoApproveMinTurnover1Y = CurrentValues.Instance.AutoApproveMinTurnover1Y;

			var mpAnalysis = GetAnalysisValsForCustomer(customerId);

			int turnover1M = (int)GetTurnoverForPeriod(mpAnalysis, TimePeriodEnum.Month);
			if (turnover1M < autoApproveMinTurnover1M)
			{
				log.InfoFormat("No auto approval: Minimal 1 month turnover for auto approval is: {0}. Customer 1 month turnover is:{1}", autoApproveMinTurnover1M, turnover1M);
				return false;
			}

			int turnover3M = (int)GetTurnoverForPeriod(mpAnalysis, TimePeriodEnum.Month3);
			if (turnover3M < autoApproveMinTurnover3M)
			{
				log.InfoFormat("No auto approval: Minimal 3 months turnover for auto approval is: {0}. Customer 3 months turnover is:{1}", autoApproveMinTurnover3M, turnover3M);
				return false;
			}

			int turnover1Y = (int)GetTurnoverForPeriod(mpAnalysis, TimePeriodEnum.Year);
			if (turnover1Y < autoApproveMinTurnover1Y)
			{
				log.InfoFormat("No auto approval: Minimal 1 year turnover for auto approval is: {0}. Customer 1 year turnover is:{1}", autoApproveMinTurnover1Y, turnover1Y);
				return false;
			}

			return true;
		}

		private bool CheckSeniority(Customer customer)
		{
			int autoApproveMinMpSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays;

			int marketplaceSeniorityInDays = MarketplaceSeniority(customer);
			if (marketplaceSeniorityInDays < autoApproveMinMpSeniorityDays)
			{
				log.InfoFormat("No auto approval: Minimal marketplace seniority for auto approval is: {0}. Customer marketplace seniority is:{1}", autoApproveMinMpSeniorityDays, marketplaceSeniorityInDays);
				return false;
			}

			return true;
		}

		private bool CheckOutstandingOffers(decimal outstandingOffers)
		{
			int autoApproveMaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers;
			if (outstandingOffers >= autoApproveMaxOutstandingOffers)
			{
				log.InfoFormat("No auto approval: Maximal allowed outstanding offers for auto approval is: {0}. Outstanding offers amount is:{1}", autoApproveMaxOutstandingOffers, outstandingOffers);
				return false;
			}

			return true;
		}

		private bool CheckTodaysLoans()
		{
			int autoApproveMaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans;
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
			int autoApproveMaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals;
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
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;
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
			int autoApproveMaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans;
			decimal autoApproveMinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion;

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

		private bool CheckWorstCaisStatus(List<string> allowedStatuses, List<string> consumerCaisDetailWorstStatuses)
		{
			log.InfoFormat("checking worst cais status");

			foreach (string consumerCaisDetailWorstStatus in consumerCaisDetailWorstStatuses)
			{
				if (!allowedStatuses.Contains(consumerCaisDetailWorstStatus))
				{
					log.InfoFormat("No auto approval: Worst CAIS status is {0}. Allowed CAIS statuses are: {1}", consumerCaisDetailWorstStatus, allowedStatuses.Aggregate((i, j) => i + "," + j));
					return false;
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
				var mail = new Mail();
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
			//TODO add flag customer address - isOwned for simple query and in case customer changes his personal address to have the owned address list correct
			var customerAddress = customerAddressRepository.GetAll()
				.Where(a => a.Customer.Id == customerId && (a.AddressType == CustomerAddressType.OtherPropertyAddress ||
					(a.AddressType == CustomerAddressType.PersonalAddress && a.Customer.PropertyStatus.IsOwnerOfMainAddress)));

			log.InfoFormat("Fetching zoopla data for {0} addresses", customerAddress.Count());

			if (customerAddress.Any())
			{
				foreach (var address in customerAddress)
				{
					if (!address.Zoopla.Any() || reCheck)
					{
						var zooplaApi = new ZooplaApi();
						try
						{
							var areaValueGraphs = zooplaApi.GetAreaValueGraphs(address.Postcode);
							var averageSoldPrices = zooplaApi.GetAverageSoldPrices(address.Postcode);
							var zooplaEstimate = zooplaApi.GetZooplaEstimate(address.ZooplaAddress);
							var regexObj = new Regex(@"[^\d]");
							var stringVal = string.IsNullOrEmpty(zooplaEstimate) ? "" : regexObj.Replace(zooplaEstimate.Trim(), "");
							int intVal;
							if (!int.TryParse(stringVal, out intVal))
							{
								intVal = 0;
							}
							address.Zoopla.Add(new Zoopla
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
								CustomerAddress = address,
								ZooplaEstimate = zooplaEstimate,
								ZooplaEstimateValue = intVal,
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

		public LandRegistryDataModel GetLandRegistryData(int customerId, string titleNumber, out LandRegistry landRegistry)
		{
			log.DebugFormat("GetLandRegistryData begin cId {0} titleNumber {1}", customerId, titleNumber);
			var lrRepo = ObjectFactory.GetInstance<LandRegistryRepository>();

			//check cash
			var cache = lrRepo.GetRes(customerId, titleNumber);
			if (cache != null)
			{
				var b = new LandRegistryModelBuilder();
				var cacheModel = new LandRegistryDataModel
				{
					Request = cache.Request,
					Response = cache.Response,
					Res = b.BuildResModel(cache.Response),
					RequestType = cache.RequestType,
					ResponseType = cache.ResponseType,
					DataSource = LandRegistryDataSource.Cache
				};

				if (!cache.Owners.Any())
				{
					var owners = new List<LandRegistryOwner>();
					foreach (var owner in cacheModel.Res.Proprietorship.ProprietorshipParties)
					{
						owners.Add(new LandRegistryOwner
						{
							LandRegistry = cache,
							FirstName = owner.PrivateIndividualForename,
							LastName = owner.PrivateIndividualSurname,
							CompanyName = owner.CompanyName,
							CompanyRegistrationNumber = owner.CompanyRegistrationNumber,
						});
					}
					cache.Owners = owners;

					lrRepo.SaveOrUpdate(cache);
				}

				landRegistry = cache;
				return cacheModel;
			}

			var isProd = CurrentValues.Instance.LandRegistryProd;

			ILandRegistryApi lr;
			if (isProd)
			{
				lr = new LandRegistryApi(
					CurrentValues.Instance.LandRegistryUserName,
					Encrypted.Decrypt(CurrentValues.Instance.LandRegistryPassword),
					CurrentValues.Instance.LandRegistryFilePath);
			}
			else
			{
				lr = new LandRegistryTestApi();
			}

			LandRegistryDataModel model;
			if (titleNumber != null)
			{
				model = lr.Res(titleNumber, customerId);
				var customer = _customers.Get(customerId);
				var dbLr = new LandRegistry
					{
						Customer = customer,
						InsertDate = DateTime.UtcNow,
						TitleNumber = titleNumber,
						Request = model.Request,
						Response = model.Response,
						RequestType = model.RequestType,
						ResponseType = model.ResponseType,
					};

				var owners = new List<LandRegistryOwner>();

				if (model.ResponseType == LandRegistryResponseType.Success && model.Res != null && model.Res.Proprietorship != null && model.Res.Proprietorship.ProprietorshipParties != null)
				{
					foreach (var owner in model.Res.Proprietorship.ProprietorshipParties)
					{
						owners.Add(new LandRegistryOwner
							{
								LandRegistry = dbLr,
								FirstName = owner.PrivateIndividualForename,
								LastName = owner.PrivateIndividualSurname,
								CompanyName = owner.CompanyName,
								CompanyRegistrationNumber = owner.CompanyRegistrationNumber,
							});
					}
					dbLr.Owners = owners;
				}
				lrRepo.Save(dbLr);
				landRegistry = dbLr;

				if (model.Attachment != null)
				{
					var fileRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_AlertDocument>>();
					var doc = new MP_AlertDocument
						{
							BinaryBody = model.Attachment.AttachmentContent,
							Customer = customer,
							Description = "LandRegistry",
							UploadDate = DateTime.UtcNow,
							DocName = model.Attachment.FileName
						};

					fileRepo.SaveOrUpdate(doc);

					model.Attachment.AttachmentContent = null;
				}
			}
			else
			{
				landRegistry = null;
				model = new LandRegistryDataModel
					{
						Res = new LandRegistryResModel { Rejection = new LandRegistryRejectionModel { Reason = "Please perform enquiry first to retrieve title number" } },
						ResponseType = LandRegistryResponseType.None
					};
			}

			model.DataSource = LandRegistryDataSource.Api;
			return model;
		}

		public static bool AreEqual(string a, string b) {
			return string.IsNullOrEmpty(a) ? string.IsNullOrEmpty(b) : string.Equals(a, b);
		}

		public LandRegistryDataModel GetLandRegistryEnquiryData(int customerId, string buildingNumber, string buildingName, string streetName, string cityName, string postCode)
		{
			var lrRepo = ObjectFactory.GetInstance<LandRegistryRepository>();

			try
			{
				//check cache
				var cache = lrRepo.GetAll().Where(x => x.Customer.Id == customerId && x.RequestType == LandRegistryRequestType.Enquiry);

				if (cache.Any())
				{
					foreach (var landRegistry in cache)
					{
						var lrReq = Serialized.Deserialize<LandRegistryLib.LREnquiryServiceNS.RequestSearchByPropertyDescriptionV2_0Type>(landRegistry.Request);
						var lrAddress = lrReq.Product.SubjectProperty.Address;

						if (AreEqual(lrAddress.BuildingName, buildingName) &&
							AreEqual(lrAddress.BuildingNumber, buildingNumber) &&
							AreEqual(lrAddress.CityName, cityName) &&
							AreEqual(lrAddress.PostcodeZone, postCode) &&
							AreEqual(lrAddress.StreetName, streetName))
						{
							var b = new LandRegistryModelBuilder();
							var cacheModel = new LandRegistryDataModel
							{
								Request = landRegistry.Request,
								Response = landRegistry.Response,
								Enquery = b.BuildEnquiryModel(landRegistry.Response),
								RequestType = landRegistry.RequestType,
								ResponseType = landRegistry.ResponseType,
								DataSource = LandRegistryDataSource.Cache,
							};
							return cacheModel;
						}
					}
				}
			}
			catch (Exception ex)
			{
				log.WarnFormat("Failed to retreive land registry enquiry from cache {0}", ex);
			}

			bool isProd = CurrentValues.Instance.LandRegistryProd;

			ILandRegistryApi lr;
			if (isProd)
			{
				lr = new LandRegistryApi(
					CurrentValues.Instance.LandRegistryUserName,
					Encrypted.Decrypt(CurrentValues.Instance.LandRegistryPassword),
					CurrentValues.Instance.LandRegistryFilePath);
			}
			else
			{
				lr = new LandRegistryTestApi();
			}

			var model = lr.EnquiryByPropertyDescription(buildingNumber, buildingName, streetName, cityName, postCode, customerId);
			Customer customer = _customers.Get(customerId);

			lrRepo.Save(new LandRegistry
				{
					Customer = customer,
					InsertDate = DateTime.UtcNow,
					Postcode = string.IsNullOrEmpty(postCode) ? string.Format("{3}{0},{1},{2}", buildingNumber, streetName, cityName, buildingName) : postCode,
					Request = model.Request,
					Response = model.Response,
					RequestType = model.RequestType,
					ResponseType = model.ResponseType
				});
			model.DataSource = LandRegistryDataSource.Api;
			return model;
		}

		public MpsTotals GetMpsTotals(int customerId)
		{
			var customer = _customers.Get(customerId);
			var totals = new MpsTotals
				{
					TotalSumOfOrders1YTotal = GetAnualTurnOverByCustomer(customer.Id),
					TotalSumOfOrders3MTotal = GetTotalSumOfOrders3M(customer.Id),
					MarketplaceSeniorityDays = MarketplaceSeniority(customer),
					TotalSumOfOrdersForLoanOffer = (decimal)GetTotalSumOfOrdersForLoanOffer(customer.Id),
					TotalSumOfOrders1YTotalForRejection = GetTurnoverForRejection(customer.Id, GetAnnualizedTurnoverFromValues),
					TotalSumOfOrders3MTotalForRejection = GetTurnoverForRejection(customer.Id, Get3MTurnoverFromValues),
					Yodlee1YForRejection = GetYodleeForRejection(customer.Id, GetAnnualizedTurnoverFromValues),
					Yodlee3MForRejection = GetYodleeForRejection(customer.Id, Get3MTurnoverFromValues)
				};
			return totals;
		}

		private bool IsOwner(Customer customer, string response, string titleNumber)
		{
			if (customer == null) {
				log.Warn("IsOwner: returning false because customer is null.");
				return false;
			} // if

			if ((customer.PersonalInfo == null) || string.IsNullOrWhiteSpace(customer.PersonalInfo.Fullname)) {
				log.WarnFormat("IsOwner: returning false for customer {0} because full name is null or empty.", customer.Id);
				return false;
			} // if

			var b = new LandRegistryModelBuilder();
			var lrData = b.BuildResModel(response, titleNumber);

			foreach (ProprietorshipPartyModel proprietorshipParty in lrData.Proprietorship.ProprietorshipParties)
			{
				// We are taking the first part of the LR first name as it may contain both first and middle name, while we might be missing the middle name
				string firstPartOfFirstName = proprietorshipParty.PrivateIndividualForename;
				int indexOfSpace = firstPartOfFirstName.IndexOf(' ');
				if (indexOfSpace != -1)
				{
					firstPartOfFirstName = firstPartOfFirstName.Substring(0, indexOfSpace);
				}

				string lowerCasedFullName = customer.PersonalInfo.Fullname.ToLower();
				if (lowerCasedFullName.Contains(firstPartOfFirstName.ToLower()) &&
					lowerCasedFullName.Contains(proprietorshipParty.PrivateIndividualSurname.ToLower())
				) {
					// Customer is owner
					return true;
				}
			}

			return false;
		}

		public void LinkLandRegistryAndAddress(int customerId, string response, string titleNumber, int landRegistryId)
		{
			var customer = _customers.Get(customerId);
			var bb = new LandRegistryModelBuilder();
			LandRegistryResModel landRegistryResModel = bb.BuildResModel(response);
			bool isOwnerAccordingToLandRegistry = IsOwner(customer, response, titleNumber);
			if (isOwnerAccordingToLandRegistry)
			{
				var ownedProperties = new List<CustomerAddress>(customer.AddressInfo.OtherPropertiesAddresses);
				if (customer.PropertyStatus.IsOwnerOfMainAddress)
				{
					if (customer.AddressInfo.PersonalAddress.Count == 1)
					{
						ownedProperties.Add(customer.AddressInfo.PersonalAddress.First());
					}
				}

				foreach (CustomerAddress ownedProperty in ownedProperties)
				{
					bool foundMatching = false;
					foreach (LandRegistryAddressModel propertyAddress in landRegistryResModel.PropertyAddresses)
					{
						if (propertyAddress.PostCode == ownedProperty.Postcode)
						{
							foundMatching = true;
							break;
						}
					}

					if (foundMatching)
					{
						ownedProperty.IsOwnerAccordingToLandRegistry = true;
						customerAddressRepository.SaveOrUpdate(ownedProperty);

						LandRegistry dbLandRegistry = landRegistryRepository.Get(landRegistryId);
						dbLandRegistry.CustomerAddress = ownedProperty;
						break;
					}
				}
			}
		}

		public void GetLandRegistryData(int customerId, List<CustomerAddressModel> addresses)
		{
			foreach (CustomerAddressModel address in addresses)
			{
				LandRegistryDataModel model = null;
				if (!string.IsNullOrEmpty(address.HouseName))
				{
					model = GetLandRegistryEnquiryData(customerId, null, address.HouseName, null, null,
													   address.PostCode);
				}

				else if (!string.IsNullOrEmpty(address.HouseNumber))
				{
					model = GetLandRegistryEnquiryData(customerId, address.HouseNumber, null, null, null,
													   address.PostCode);
				}

				else if (!string.IsNullOrEmpty(address.FlatOrApartmentNumber) &&
						 string.IsNullOrEmpty(address.HouseNumber))
				{
					model = GetLandRegistryEnquiryData(customerId, address.FlatOrApartmentNumber, null, null, null,
													   address.PostCode);
				}

				if (model != null && model.Enquery != null && model.ResponseType == LandRegistryResponseType.Success &&
					model.Enquery.Titles.Any() && model.Enquery.Titles.Count == 1)
				{
					LandRegistry dbLandRegistry;
					LandRegistryDataModel landRegistryDataModel = GetLandRegistryData(customerId, model.Enquery.Titles[0].TitleNumber, out dbLandRegistry);

					if (landRegistryDataModel.ResponseType == LandRegistryResponseType.Success)
					{
						// Verify customer is among owners
						Customer customer = _customers.Get(customerId);
						bool isOwnerAccordingToLandRegistry = IsOwner(customer, landRegistryDataModel.Response, landRegistryDataModel.Res.TitleNumber);
						CustomerAddress dbAdress = customerAddressRepository.Get(address.AddressId);

						dbLandRegistry.CustomerAddress = dbAdress;
						landRegistryRepository.SaveOrUpdate(dbLandRegistry);

						if (isOwnerAccordingToLandRegistry)
						{
							dbAdress.IsOwnerAccordingToLandRegistry = true;
							customerAddressRepository.SaveOrUpdate(dbAdress);
						}
					}
				}
				else
				{
					int num = 0;
					if (model != null && model.Enquery != null && model.Enquery.Titles.Any())
					{
						num = model.Enquery.Titles.Count;
					}
					log.WarnFormat(
						"No land registry retrieved for customer id: {5}, house name: {0}, house number: {1}, flat number: {2}, postcode: {3}, num of enquries {4}",
						address.HouseName, address.HouseNumber,
						address.FlatOrApartmentNumber, address.PostCode, num, customerId);
				}
			}
		}

	}
}