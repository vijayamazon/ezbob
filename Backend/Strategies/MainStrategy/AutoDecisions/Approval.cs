namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoApproval;
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
		public Approval(
			int customerId,
			bool bIsBrokerCustomer,
			int minExperianScore,
			int minCompanyScore,
			int offeredCreditLine,
			List<string> consumerCaisDetailWorstStatuses,
			AConnection db,
			ASafeLog log
		) {
			this.db = db;
			this.log = log;
			this.minExperianScore = minExperianScore;
			this.minCompanyScore = minCompanyScore;
			this.autoApprovedAmount = offeredCreditLine;
			this.customerId = customerId;
			this.consumerCaisDetailWorstStatuses = consumerCaisDetailWorstStatuses;
			this.isBrokerCustomer = bIsBrokerCustomer;

			loanRepository = ObjectFactory.GetInstance<LoanRepository>();
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
			cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
			experianDefaultAccountRepository = ObjectFactory.GetInstance<ExperianDefaultAccountRepository>();
			loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();

			customer = _customers.ReallyTryGet(customerId);

			m_oTrail = new Trail(customerId);
		} // constructor

		public void MakeDecision(AutoDecisionResponse response) {
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
						} // if is silent
					}
					else {
						response.CreditResult = "WaitingForDecision";
						response.UserStatus = "Manual";
						response.SystemDecision = "Manual";
					} // if there are enough funds
				} // if auto approved amount is not 0
			}
			catch (Exception e) {
				log.Error(e, "Exception during auto approval.");
			} // try
		} // MakeDecision

		private void CheckAutoApprovalConformance(decimal outstandingOffers) {
			m_oTrail.Append<InitialAssignment>(this.autoApprovedAmount > 0).Init(this.autoApprovedAmount);

			log.Debug("Checking if auto approval should take place for customer {0}...", customerId);

			try {
				m_oTrail.Append<IsBrokerCustomer>(!isBrokerCustomer);

				CheckAMLResult();
				CheckBusinessScore();
				CheckCustomerStatus();
				CheckExperianScore();
				CheckAge();
				CheckTurnovers(); // TODO: print to log or new step detailed turnover
				CheckSeniority(); // TODO: print to log or new step detailed seniority
				CheckOutstandingOffers(outstandingOffers);
				CheckTodaysLoans();
				CheckTodaysApprovals();
				CheckDefaultAccounts();

				// TODO: create new step "total number of loans" which is always passed just to indicate number of loans

				if (!loanRepository.ByCustomer(customerId).Any())
					CheckWorstCaisStatus(new List<string> { "0", "1", "2" }); // Up to 60 days
				else {
					CheckWorstCaisStatus(new List<string> { "0", "1", "2", "3" }); // Up to 90 days
					CheckRollovers();
					CheckLateDays();
					decimal outstandingPrincipal = CheckOutstandingLoans();

					// Reduce the system calculated amount by the already open amount
					autoApprovedAmount -= (int)outstandingPrincipal;

					StepDone<ReduceOutstandingPrincipal>().Init(outstandingPrincipal, autoApprovedAmount);
				} // if

				int autoApproveMinAmount = CurrentValues.Instance.AutoApproveMinAmount;
				int autoApproveMaxAmount = CurrentValues.Instance.AutoApproveMaxAmount;

				if (autoApprovedAmount < autoApproveMinAmount || autoApprovedAmount > autoApproveMaxAmount)
					StepFailed<AmountOutOfRangle>().Init(autoApprovedAmount, autoApproveMinAmount, autoApproveMaxAmount);
				else
					StepDone<AmountOutOfRangle>().Init(autoApprovedAmount, autoApproveMinAmount, autoApproveMaxAmount);
			}
			catch (Exception ex) {
				StepFailed<ExceptionThrown>().Init(ex);
			} // try

			m_oTrail.Append<Complete>(autoApprovedAmount > 0).Init(autoApprovedAmount);

			log.Debug("Checking if auto approval should take place for customer {0} complete.", customerId);

			log.Msg("Auto approved amount: {0}. {1}", autoApprovedAmount, m_oTrail);
		} // CheckAutoApprovalConformance

		private void CheckDefaultAccounts() {
			if (experianDefaultAccountRepository.GetAll().Any(entry => entry.Customer.Id == customerId))
				StepFailed<DefaultAccounts>().Init();
			else
				StepDone<DefaultAccounts>().Init();
		} // CheckDefaultAccounts

		private void CheckAMLResult() {
			if (customer == null)
				StepFailed<AmlCheck>().Init("failed because customer not found");
			else if (customer.AMLResult != "Passed")
				StepFailed<AmlCheck>().Init(customer.AMLResult);
			else
				StepDone<AmlCheck>().Init(customer.AMLResult);
		} // CheckAMLResult

		private void CheckCustomerStatus() {
			if (customer == null)
				StepFailed<CustomerStatus>().Init("unknown");
			else if (!customer.CollectionStatus.CurrentStatus.IsEnabled)
				StepFailed<CustomerStatus>().Init(customer.CollectionStatus.CurrentStatus.Name);
			else
				StepDone<CustomerStatus>().Init(customer.CollectionStatus.CurrentStatus.Name);
		} // CheckCustomerStatus

		private void CheckBusinessScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold;

			if (minCompanyScore < nThreshold)
				StepFailed<BusinessScore>().Init(minCompanyScore, nThreshold);
			else
				StepDone<BusinessScore>().Init(minCompanyScore, nThreshold);
		} // CheckBusinessScore

		private void CheckExperianScore() {
			int autoApproveExperianScoreThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold;

			if (minExperianScore < autoApproveExperianScoreThreshold)
				StepFailed<BusinessScore>().Init(minExperianScore, autoApproveExperianScoreThreshold);
			else
				StepDone<BusinessScore>().Init(minExperianScore, autoApproveExperianScoreThreshold);
		} // CheckExperianScore

		private void CheckAge() {
			int autoApproveCustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge;
			int autoApproveCustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge;

			if (customer == null)
				StepFailed<Age>().Init(-1, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
			else if (customer.PersonalInfo.DateOfBirth != null) {
				DateTime now = DateTime.UtcNow;

				int customerAge = now.Year - customer.PersonalInfo.DateOfBirth.Value.Year;

				if (now < customer.PersonalInfo.DateOfBirth.Value.AddYears(customerAge))
					customerAge--;

				if (customerAge < autoApproveCustomerMinAge || customerAge > autoApproveCustomerMaxAge)
					StepFailed<Age>().Init(customerAge, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
				else
					StepDone<Age>().Init(customerAge, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
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

			Turnover oTrace;

			if (turnover < nThreshold)
				oTrace = (Turnover)StepFailed<Turnover>();
			else
				oTrace = (Turnover)StepDone<Turnover>();

			oTrace.PeriodName = nPeriod.ToString();
			oTrace.Init(turnover, nThreshold);
		} // CheckOnePeriodTurnover

		private void CheckSeniority() {
			int autoApproveMinMpSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays;

			if (customer == null)
				StepFailed<MarketplaceSeniority>().Init(-1, autoApproveMinMpSeniorityDays);
			else {
				int marketplaceSeniorityInDays = strategyHelper.MarketplaceSeniority(customer);

				if (marketplaceSeniorityInDays < autoApproveMinMpSeniorityDays)
					StepFailed<MarketplaceSeniority>().Init(marketplaceSeniorityInDays, autoApproveMinMpSeniorityDays);
				else
					StepDone<MarketplaceSeniority>().Init(marketplaceSeniorityInDays, autoApproveMinMpSeniorityDays);
			} // if
		} // CheckSeniority

		private void CheckOutstandingOffers(decimal outstandingOffers) {
			int autoApproveMaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers;

			if (outstandingOffers >= autoApproveMaxOutstandingOffers)
				StepFailed<OutstandingOffers>().Init(outstandingOffers, autoApproveMaxOutstandingOffers);
			else
				StepDone<OutstandingOffers>().Init(outstandingOffers, autoApproveMaxOutstandingOffers);
		} // CheckOutstandingOffers

		private void CheckTodaysLoans() {
			int autoApproveMaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans;

			DateTime today = DateTime.UtcNow;

			var todayLoans = loanRepository.GetAll().Where(l => l.Date.Year == today.Year && l.Date.Month == today.Month && l.Date.Day == today.Day);

			decimal todayLoansAmount = 0;

			if (todayLoans.Any())
				todayLoansAmount = todayLoans.Sum(l => l.LoanAmount);

			if (todayLoansAmount >= autoApproveMaxTodayLoans)
				StepFailed<TodayLoans>().Init(todayLoansAmount, autoApproveMaxTodayLoans);
			else
				StepDone<TodayLoans>().Init(todayLoansAmount, autoApproveMaxTodayLoans);
		} // CheckTodaysLoans

		private void CheckTodaysApprovals() {
			int autoApproveMaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals;

			DateTime today = DateTime.UtcNow;

			int numOfApprovalsToday = cashRequestsRepository.GetAll().Count(cr => cr.CreationDate.HasValue && cr.CreationDate.Value.Year == today.Year && cr.CreationDate.Value.Month == today.Month && cr.CreationDate.Value.Day == today.Day && cr.UnderwriterComment == "Auto Approval");

			if (numOfApprovalsToday >= autoApproveMaxDailyApprovals)
				StepFailed<TodayApprovalCount>().Init(numOfApprovalsToday, autoApproveMaxDailyApprovals);
			else
				StepDone<TodayApprovalCount>().Init(numOfApprovalsToday, autoApproveMaxDailyApprovals);
		} // CheckTodaysApprovals

		private void CheckRollovers() {
			if (loanRepository.ByCustomer(customerId).Any(l => l.Schedule.Any(s => s.Rollovers.Any())))
				StepFailed<Rollovers>().Init();
			else
				StepDone<Rollovers>().Init();
		} // CheckRollovers

		private void CheckLateDays() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			List<int> customerLoanIds = loanRepository.ByCustomer(customerId).Select(d => d.Id).ToList();
			bool bSuccess = true;

			foreach (int loanId in customerLoanIds) {
				int innerLoanId = loanId;

				var backfilledMapping = loanScheduleTransactionRepository.GetAll().Where(x => x.Loan.Id == innerLoanId);

				foreach (var paymentMapping in backfilledMapping) {
					var scheduleDate = new DateTime(paymentMapping.Schedule.Date.Year, paymentMapping.Schedule.Date.Month, paymentMapping.Schedule.Date.Day);

					var transactionDate = new DateTime(paymentMapping.Transaction.PostDate.Year, paymentMapping.Transaction.PostDate.Month, paymentMapping.Transaction.PostDate.Day);

					double nTotalLateDays = transactionDate.Subtract(scheduleDate).TotalDays;

					if (nTotalLateDays > autoApproveMaxAllowedDaysLate) {
						bSuccess = false;
						StepFailed<LateDays>().Init((int)nTotalLateDays, innerLoanId, paymentMapping.Schedule.Id, paymentMapping.Transaction.Id, autoApproveMaxAllowedDaysLate);
					} // if
				} // for
			} // for

			if (bSuccess)
				StepDone<LateDays>().Init(0, 0, 0, 0, autoApproveMaxAllowedDaysLate);
		} // CheckLateDays

		private decimal CheckOutstandingLoans() {
			int autoApproveMaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans;
			decimal autoApproveMinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion;

			List<Loan> outstandingLoans = strategyHelper.GetOutstandingLoans(customerId);

			if (outstandingLoans.Count > autoApproveMaxNumOfOutstandingLoans)
				StepFailed<OutstandingLoanCount>().Init(outstandingLoans.Count, autoApproveMaxNumOfOutstandingLoans);
			else
				StepDone<OutstandingLoanCount>().Init(outstandingLoans.Count, autoApproveMaxNumOfOutstandingLoans);

			decimal loanAmount = 0;
			decimal outstandingPrincipal = 0;

			foreach (var loan in outstandingLoans) {
				loanAmount += loan.LoanAmount;
				outstandingPrincipal += loan.Principal;
			} // for

			if (outstandingPrincipal != 0 && outstandingPrincipal >= autoApproveMinRepaidPortion * loanAmount)
				StepFailed<OutstandingRepayRatio>().Init(loanAmount == 0 ? 0 : outstandingPrincipal / loanAmount, autoApproveMinRepaidPortion);
			else
				StepDone<OutstandingRepayRatio>().Init(loanAmount == 0 ? 0 : outstandingPrincipal / loanAmount, autoApproveMinRepaidPortion);

			return outstandingPrincipal;
		} // CheckOutstandingLoans

		private void CheckWorstCaisStatus(List<string> allowedStatuses) {
			List<string> diff = consumerCaisDetailWorstStatuses.Except(allowedStatuses).ToList();

			if (diff.Count > 1)
				StepFailed<WorstCaisStatus>().Init(diff, consumerCaisDetailWorstStatuses, allowedStatuses);
			else
				StepDone<WorstCaisStatus>().Init(null, consumerCaisDetailWorstStatuses, allowedStatuses);
		} // CheckWorstCaisStatus

		private T StepFailed<T>() where T : ATrace {
			autoApprovedAmount = 0;
			return m_oTrail.Failed<T>();
		} // StepFailed

		private T StepDone<T>() where T : ATrace {
			return m_oTrail.Done<T>();
		} // StepFailed

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
		private readonly bool isBrokerCustomer;
		private readonly AConnection db;
		private readonly Customer customer;
		private readonly int minExperianScore;
		private readonly int minCompanyScore;
		private readonly int customerId;
		private readonly List<string> consumerCaisDetailWorstStatuses;

		private int autoApprovedAmount;

		private readonly Trail m_oTrail;

		private readonly ASafeLog log;
	} // class Approval
} // namespace
