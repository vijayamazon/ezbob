namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Experian;
	using EzBob.Backend.Strategies.Misc;
	using EzBob.CommonLib.TimePeriodLogic;
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailApi;
	using StructureMap;

	public class Approval {
		public Approval(int customerId, int minExperianScore, int minCompanyScore, int offeredCreditLine, List<string> consumerCaisDetailWorstStatuses, AConnection db, ASafeLog log) {
			this.db = db;
			this.log = log;
			this.minExperianScore = minExperianScore;
			this.minCompanyScore = minCompanyScore;
			this.autoApprovedAmount = offeredCreditLine;
			this.customerId = customerId;
			this.consumerCaisDetailWorstStatuses = consumerCaisDetailWorstStatuses;

			loanRepository = ObjectFactory.GetInstance<LoanRepository>();
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
			cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
			experianDefaultAccountRepository = ObjectFactory.GetInstance<ExperianDefaultAccountRepository>();
			loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();

			customer = _customers.ReallyTryGet(customerId);
		} // constructor

		public bool MakeDecision(AutoDecisionResponse response) {
			try {
				var configSafeReader = db.GetFirst("GetApprovalConfigs", CommandSpecies.StoredProcedure);

				bool autoApproveIsSilent = configSafeReader["AutoApproveIsSilent"];
				string autoApproveSilentTemplateName = configSafeReader["AutoApproveSilentTemplateName"];
				string autoApproveSilentToAddress = configSafeReader["AutoApproveSilentToAddress"];
				decimal minLoanAmount = configSafeReader["MinLoanAmount"];

				var availFunds = new GetAvailableFunds(db, log);
				availFunds.Execute();

				CheckAutoApprovalConformance(availFunds.ReservedAmount);
				response.AutoApproveAmount = autoApprovedAmount;

				response.AutoApproveAmount = (int)(
					Math.Round(response.AutoApproveAmount / minLoanAmount, 0, MidpointRounding.AwayFromZero) * minLoanAmount
				);

				log.Info("Decided to auto approve rounded amount: {0}", response.AutoApproveAmount);

				if (response.AutoApproveAmount != 0) {
					if (availFunds.AvailableFunds > response.AutoApproveAmount) {
						if (autoApproveIsSilent) {
							NotifyAutoApproveSilentMode(response.AutoApproveAmount, autoApproveSilentTemplateName, autoApproveSilentToAddress);

							response.CreditResult = "WaitingForDecision";
							response.UserStatus = "Manual";
							response.SystemDecision = "Manual";
						}
						else {
							SafeReader sr = db.GetFirst(
								"GetLastOfferDataForApproval",
								CommandSpecies.StoredProcedure,
								new QueryParameter("CustomerId", customerId),
								new QueryParameter("Now", DateTime.UtcNow)
							);

							bool loanOfferEmailSendingBanned = sr["EmailSendingBanned"];
							DateTime loanOfferOfferStart = sr["OfferStart"];
							DateTime loanOfferOfferValidUntil = sr["OfferValidUntil"];

							response.CreditResult = "Approved";
							response.UserStatus = "Approved";
							response.SystemDecision = "Approve";
							response.LoanOfferUnderwriterComment = "Auto Approval";
							response.DecisionName = "Approval";
							response.AppValidFor = DateTime.UtcNow.AddDays((loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays);
							response.IsAutoApproval = true;
							response.LoanOfferEmailSendingBannedNew = loanOfferEmailSendingBanned;
						} // if
					}
					else {
						response.CreditResult = "WaitingForDecision";
						response.UserStatus = "Manual";
						response.SystemDecision = "Manual";
					} // if

					return true;
				} // if

				return false;
			}
			catch (Exception e) {
				log.Error(e, "Exception during auto approval.");
				return false;
			} // try
		} // MakeDecision

		private void CheckAutoApprovalConformance(decimal outstandingOffers) {
			log.Info("Checking if auto approval should take place...");

			try {
				CheckAMLResult();
				CheckBusinessScore();
				CheckCustomerStatus();
				CheckExperianScore();
				CheckAge();
				CheckTurnovers();
				CheckSeniority();
				CheckOutstandingOffers(outstandingOffers);
				CheckTodaysLoans();
				CheckTodaysApprovals();
				CheckDefaultAccounts();

				if (!loanRepository.ByCustomer(customerId).Any())
					CheckWorstCaisStatus(new List<string> { "0", "1", "2" }); // Up to 60 days
				else {
					CheckWorstCaisStatus(new List<string> { "0", "1", "2", "3" }); // Up to 90 days
					CheckRollovers();
					CheckLateDays();
					decimal outstandingPrincipal = CheckOutstandingLoans();

					// Reduce the system calculated amount by the already open amount
					autoApprovedAmount -= (int)outstandingPrincipal;
				} // if

				int autoApproveMinAmount = CurrentValues.Instance.AutoApproveMinAmount;
				int autoApproveMaxAmount = CurrentValues.Instance.AutoApproveMaxAmount;

				if (autoApprovedAmount < autoApproveMinAmount || autoApprovedAmount > autoApproveMaxAmount) {
					log.Info("No auto approval: System calculated amount is not between {0}-{1} but is {2}", autoApproveMinAmount, autoApproveMaxAmount, autoApprovedAmount);
					autoApprovedAmount = 0;
				} // if
			}
			catch (Exception ex) {
				log.Error(ex, "No auto approval: Exception while checking auto approval conditions.");
				autoApprovedAmount = 0;
			} // try

			log.Info("Calculated auto approve amount:{0}", autoApprovedAmount);
		} // CheckAutoApprovalConformance

		private void CheckDefaultAccounts() {
			if (experianDefaultAccountRepository.GetAll().Any(entry => entry.Customer.Id == customerId)) {
				log.Info("No auto approval: No auto approval for customer with default accounts");
				autoApprovedAmount = 0;
			} // if
		} // CheckDefaultAccounts

		private void CheckAMLResult() {
			if (customer == null) {
				log.Info("No auto approval: AML is not checked because customer not found by id {0}", customerId);
				autoApprovedAmount = 0;
			}
			else if (customer.AMLResult != "Passed") {
				log.Info("No auto approval: AML is not passed");
				autoApprovedAmount = 0;
			} // if
		} // CheckAMLResult

		private void CheckCustomerStatus() {
			if (customer == null) {
				log.Info("No auto approval: customer status not checked because customer not found by id {0}", customerId);
				autoApprovedAmount = 0;
			}
			else if (!customer.CollectionStatus.CurrentStatus.IsEnabled) {
				log.Info("No auto approval: Only 'enabled' statuses are allowed for auto approval. Customer status is:{0}", customer.CollectionStatus.CurrentStatus.Name);
				autoApprovedAmount = 0;
			} // if
		} // CheckCustomerStatus

		private void CheckBusinessScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold;

			if (minCompanyScore < nThreshold) {
				log.Info("No auto approval: Minimal company score threshold is: {0}. Customer minimal company score is: {1}", nThreshold, minCompanyScore);
				autoApprovedAmount = 0;
			} // if
		} // CheckBusinessScore

		private void CheckExperianScore() {
			int autoApproveExperianScoreThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold;

			if (minExperianScore < autoApproveExperianScoreThreshold) {
				log.Info("No auto approval: Minimal score threshold is: {0}. Customer minimal score (considering directors) is:{1}", autoApproveExperianScoreThreshold, minExperianScore);
				autoApprovedAmount = 0;
			} // if
		} // CheckExperianScore

		private void CheckAge() {
			int? customerAge = null;

			if (customer == null) {
				log.Info("No auto approval: customer age not checked because customer not found by id {0}", customerId);
				autoApprovedAmount = 0;
			}
			else if (customer.PersonalInfo.DateOfBirth != null) {
				DateTime now = DateTime.UtcNow;

				customerAge = now.Year - customer.PersonalInfo.DateOfBirth.Value.Year;

				if (now < customer.PersonalInfo.DateOfBirth.Value.AddYears(customerAge.Value))
					customerAge--;
			} // if

			int autoApproveCustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge;
			int autoApproveCustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge;

			if (customerAge == null || customerAge < autoApproveCustomerMinAge || customerAge > autoApproveCustomerMaxAge) {
				log.Info("No auto approval: Customer age should be between {0}-{1} for auto approval. Customer age is {2}", autoApproveCustomerMinAge, autoApproveCustomerMaxAge, customerAge == null ? "Unknown" : customerAge.Value.ToString(CultureInfo.InvariantCulture));
				autoApprovedAmount = 0;
			} // if
		} // CheckAge

		private void CheckTurnovers() {
			Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>> mpAnalysis = strategyHelper.GetAnalysisValsForCustomer(customerId);

			CheckOnePeriodTurnover(mpAnalysis, CurrentValues.Instance.AutoApproveMinTurnover1M, TimePeriodEnum.Month);
			CheckOnePeriodTurnover(mpAnalysis, CurrentValues.Instance.AutoApproveMinTurnover3M, TimePeriodEnum.Month3);
			CheckOnePeriodTurnover(mpAnalysis, CurrentValues.Instance.AutoApproveMinTurnover1Y, TimePeriodEnum.Year);
		} // CheckTurnovers

		private void CheckOnePeriodTurnover(
			Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>> mpAnalysis,
			int nThreshold,
			TimePeriodEnum nPeriod
		) {
			int turnover = (int)strategyHelper.GetTurnoverForPeriod(mpAnalysis, nPeriod);

			if (turnover < nThreshold) {
				log.Info(
					"No auto approval: Minimal {2} turnover for auto approval is: {0}. Customer {2} turnover is: {1}.",
					nThreshold,
					turnover,
					nPeriod
				);

				autoApprovedAmount = 0;
			} // if
		} // CheckOnePeriodTurnover

		private void CheckSeniority() {
			if (customer == null) {
				log.Info("No auto approval: customer marketplace seniority not checked because customer not found by id {0}", customerId);
				autoApprovedAmount = 0;
			}
			else {
				int autoApproveMinMpSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays;

				int marketplaceSeniorityInDays = strategyHelper.MarketplaceSeniority(customer);

				if (marketplaceSeniorityInDays < autoApproveMinMpSeniorityDays) {
					log.Info("No auto approval: Minimal marketplace seniority for auto approval is: {0}. Customer marketplace seniority is:{1}", autoApproveMinMpSeniorityDays, marketplaceSeniorityInDays);
					autoApprovedAmount = 0;
				} // if
			} // if
		} // CheckSeniority

		private void CheckOutstandingOffers(decimal outstandingOffers) {
			int autoApproveMaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers;

			if (outstandingOffers >= autoApproveMaxOutstandingOffers) {
				log.Info("No auto approval: Maximal allowed outstanding offers for auto approval is: {0}. Outstanding offers amount is:{1}", autoApproveMaxOutstandingOffers, outstandingOffers);
				autoApprovedAmount = 0;
			} // if
		} // CheckOutstandingOffers

		private void CheckTodaysLoans() {
			int autoApproveMaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans;

			DateTime today = DateTime.UtcNow;

			var todayLoans = loanRepository.GetAll().Where(l => l.Date.Year == today.Year && l.Date.Month == today.Month && l.Date.Day == today.Day);

			decimal todayLoansAmount = 0;

			if (todayLoans.Any())
				todayLoansAmount = todayLoans.Sum(l => l.LoanAmount);

			if (todayLoansAmount >= autoApproveMaxTodayLoans) {
				log.Info("No auto approval: Maximal allowed today's loans for auto approval is: {0}. Today's loan amount is:{1}", autoApproveMaxTodayLoans, todayLoansAmount);
				autoApprovedAmount = 0;
			} // if
		} // CheckTodaysLoans

		private void CheckTodaysApprovals() {
			int autoApproveMaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals;

			DateTime today = DateTime.UtcNow;

			int numOfApprovalsToday = cashRequestsRepository.GetAll().Count(cr => cr.CreationDate.HasValue && cr.CreationDate.Value.Year == today.Year && cr.CreationDate.Value.Month == today.Month && cr.CreationDate.Value.Day == today.Day && cr.UnderwriterComment == "Auto Approval");

			if (numOfApprovalsToday >= autoApproveMaxDailyApprovals) {
				log.Info("No auto approval: Maximal allowed auto approvals per day is: {0}.", autoApproveMaxDailyApprovals);
				autoApprovedAmount = 0;
			} // if
		} // CheckTodaysApprovals

		private void CheckRollovers() {
			if (loanRepository.ByCustomer(customerId).Any(l => l.Schedule.Any(s => s.Rollovers.Any()))) {
				log.Info("No auto approval: No auto approval for customers with rollovers");
				autoApprovedAmount = 0;
			} // if
		} // CheckRollovers

		private void CheckLateDays() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			List<int> customerLoanIds = loanRepository.ByCustomer(customerId).Select(d => d.Id).ToList();

			foreach (int loanId in customerLoanIds) {
				int innerLoanId = loanId;

				var backfilledMapping = loanScheduleTransactionRepository.GetAll().Where(x => x.Loan.Id == innerLoanId);

				foreach (var paymentMapping in backfilledMapping) {
					var scheduleDate = new DateTime(paymentMapping.Schedule.Date.Year, paymentMapping.Schedule.Date.Month, paymentMapping.Schedule.Date.Day);

					var transactionDate = new DateTime(paymentMapping.Transaction.PostDate.Year, paymentMapping.Transaction.PostDate.Month, paymentMapping.Transaction.PostDate.Day);

					if (transactionDate.Subtract(scheduleDate).TotalDays > autoApproveMaxAllowedDaysLate) {
						log.Info("No auto approval: No auto approvals if a customer was late over {0} days. This customer was {1} days late for loan: {2} schedule: {3} transaction: {4}", autoApproveMaxAllowedDaysLate, transactionDate.Subtract(scheduleDate).TotalDays, innerLoanId, paymentMapping.Schedule.Id, paymentMapping.Transaction.Id);
						autoApprovedAmount = 0;
					} // if
				} // for
			} // for
		} // CheckLateDays

		private decimal CheckOutstandingLoans() {
			int autoApproveMaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans;
			decimal autoApproveMinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion;

			List<Loan> outstandingLoans = strategyHelper.GetOutstandingLoans(customerId);

			if (outstandingLoans.Count > autoApproveMaxNumOfOutstandingLoans) {
				log.Info("No auto approval: No auto approval for customers with more than {0} outstanding loans. This customer has {1} outstanding loans.", autoApproveMaxNumOfOutstandingLoans, outstandingLoans.Count);
				autoApprovedAmount = 0;
			} // if

			decimal loanAmount = 0;
			decimal outstandingPrincipal = 0;

			foreach (var loan in outstandingLoans) {
				loanAmount += loan.LoanAmount;
				outstandingPrincipal += loan.Principal;
			} // for

			if (outstandingPrincipal != 0 && outstandingPrincipal >= autoApproveMinRepaidPortion * loanAmount) {
				log.Info("No auto approval: No auto approval for customers that didn't repay at least {0} of their original principal. This customer has repaid {1}", autoApproveMinRepaidPortion, loanAmount == 0 ? 0 : outstandingPrincipal / loanAmount);
				autoApprovedAmount = 0;
			} // if

			return outstandingPrincipal;
		} // CheckOutstandingLoans

		private void CheckWorstCaisStatus(List<string> allowedStatuses) {
			log.Info("checking worst CAIS status");

			foreach (string consumerCaisDetailWorstStatus in consumerCaisDetailWorstStatuses) {
				if (!allowedStatuses.Contains(consumerCaisDetailWorstStatus)) {
					log.Info("No auto approval: Worst CAIS status is {0}. Allowed CAIS statuses are: {1}", consumerCaisDetailWorstStatus, allowedStatuses.Aggregate((i, j) => i + "," + j));
					autoApprovedAmount = 0;
				} // if
			} // for each
		} // CheckWorstCaisStatus

		private void NotifyAutoApproveSilentMode(int autoApproveAmount, string autoApproveSilentTemplateName, string autoApproveSilentToAddress) {
			try {
				log.Info("Sending silent auto approval mail for: customerId={0} autoApproveAmount={1} autoApproveSilentTemplateName={2} autoApproveSilentToAddress={3}", customerId, autoApproveAmount, autoApproveSilentTemplateName, autoApproveSilentToAddress);

				var vars = new Dictionary<string, string> {
					{"customerId", customerId.ToString(CultureInfo.InvariantCulture)},
					{"autoApproveAmount", autoApproveAmount.ToString(CultureInfo.InvariantCulture)}
				};

				var result = new Mail().Send(vars, autoApproveSilentToAddress, autoApproveSilentTemplateName);

				if (result == "OK")
					log.Info("Sent mail - silent auto approval");
				else
					log.Error("Failed sending alert mail - silent auto approval. Result: {0}", result);
			}
			catch (Exception e) {
				log.Error(e, "Failed sending alert mail - silent auto approval.");
			} // try
		} // NotifyAutoApproveSilentMode

		private readonly CustomerRepository _customers;
		private readonly CashRequestsRepository cashRequestsRepository;
		private readonly ExperianDefaultAccountRepository experianDefaultAccountRepository;
		private readonly LoanScheduleTransactionRepository loanScheduleTransactionRepository;
		private readonly LoanRepository loanRepository;

		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly AConnection db;
		private readonly Customer customer;
		private readonly int minExperianScore;
		private readonly int minCompanyScore;
		private readonly int customerId;
		private readonly List<string> consumerCaisDetailWorstStatuses;

		private int autoApprovedAmount;

		private readonly ASafeLog log;
	} // class Approval
} // namespace
