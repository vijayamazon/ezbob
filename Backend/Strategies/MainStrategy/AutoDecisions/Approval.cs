namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Experian;
	using MedalCalculations;
	using Misc;
	using CommonLib.TimePeriodLogic;
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailApi;
	using OfferCalculation;
	using StructureMap;

	public class Approval {
		public Approval(
			int customerId,
			bool bIsBrokerCustomer,
			int minExperianScore,
			int minCompanyScore,
			int offeredCreditLine,
			List<string> consumerCaisDetailWorstStatuses,
			bool hasLoans,
			MedalClassification medalClassification,
			AConnection db,
			ASafeLog log
		) {
			this.db = db;
			this.log = log ?? new SafeLog();
			this.minExperianScore = minExperianScore;
			this.minCompanyScore = minCompanyScore;
			this.autoApprovedAmount = offeredCreditLine;
			this.customerId = customerId;
			this.consumerCaisDetailWorstStatuses = consumerCaisDetailWorstStatuses;
			this.isBrokerCustomer = bIsBrokerCustomer;
			this.medalClassification = medalClassification;
			this.hasLoans = hasLoans;

			loanRepository = ObjectFactory.GetInstance<LoanRepository>();
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
			cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
			experianDefaultAccountRepository = ObjectFactory.GetInstance<ExperianDefaultAccountRepository>();
			loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();

			customer = _customers.ReallyTryGet(customerId);

			m_oTrail = new ApprovalTrail(customerId, this.log);

			m_oTurnover = new SortedDictionary<TimePeriodEnum, decimal>();

			m_oSecondaryImplementation = new Agent(customerId, offeredCreditLine, db, log);
		} // constructor

		public void MakeDecision(AutoDecisionResponse response) {
			try {
				bool autoApproveIsSilent = CurrentValues.Instance.AutoApproveIsSilent;
				string autoApproveSilentTemplateName = CurrentValues.Instance.AutoApproveSilentTemplateName;
				string autoApproveSilentToAddress = CurrentValues.Instance.AutoApproveSilentToAddress;
				decimal minLoanAmount = CurrentValues.Instance.MinLoanAmount;

				var availFunds = new GetAvailableFunds(db, log);
				availFunds.Execute();

				SaveTrailInputData(availFunds);

				CheckAutoApprovalConformance(availFunds.ReservedAmount);
				m_oSecondaryImplementation.MakeDecision();

				bool bSuccess = m_oTrail.EqualsTo(m_oSecondaryImplementation.Trail);

				m_oTrail.Save(db, m_oSecondaryImplementation.Trail);

				if (bSuccess) {
					log.Info("Both Auto Approval implementations have reached the same decision: {0}", m_oTrail.HasDecided ? "approved" : "not approved");
					response.AutoApproveAmount = autoApprovedAmount;
				}
				else {
					log.Alert(
						"Switching to manual decision: Auto Approval implementations " +
						"have not reached the same decision for customer {0}, diff id is {1}.",
						customerId,
						m_oTrail.DiffID.ToString("N")
					);

					response.AutoApproveAmount = 0;

					response.CreditResult = "WaitingForDecision";
					response.UserStatus = "Manual";
					response.SystemDecision = "Manual";
				} // if

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
						else
						{
							var offerDualCalculator = new OfferDualCalculator(db, log);
							OfferResult offerResult = offerDualCalculator.CalculateOffer(customerId, DateTime.UtcNow, response.AutoApproveAmount, hasLoans, medalClassification);
							if (offerResult == null || !string.IsNullOrEmpty(offerResult.Error))
							{
								log.Alert("Failed calculating offer for auto-approve error:{0}. Will use manual. Customer:{1}", offerResult != null ? offerResult.Error : "", customerId);
								response.CreditResult = "WaitingForDecision";
								response.UserStatus = "Manual";
								response.SystemDecision = "Manual";
							}
							else
							{
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

								// Use offer calculated data
								response.RepaymentPeriod = offerResult.Period;
								response.IsEu = offerResult.IsEu;
								response.LoanTypeId = offerResult.LoanTypeId;
								response.InterestRate = offerResult.InterestRate / 100;
								response.SetupFee = offerResult.SetupFee / 100;
							}
						} // if is silent
					} // if there are enough funds
				} // if auto approved amount is not 0
			}
			catch (Exception e) {
				log.Error(e, "Exception during auto approval.");
			} // try
		} // MakeDecision

		private void SaveTrailInputData(GetAvailableFunds availFunds) {
			CalculateTurnovers();
			m_oTrail.MyInputData.SetTurnover(1, m_oTurnover[TimePeriodEnum.Month]);
			m_oTrail.MyInputData.SetTurnover(3, m_oTurnover[TimePeriodEnum.Month3]);
			m_oTrail.MyInputData.SetTurnover(12, m_oTurnover[TimePeriodEnum.Year]);

			m_oTrail.MyInputData.SetDataAsOf(DateTime.UtcNow);

			m_oTrail.MyInputData.SetConfiguration(new Configuration {
				ExperianScoreThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold,
				CustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge,
				CustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge,
				MinTurnover1M = CurrentValues.Instance.AutoApproveMinTurnover1M,
				MinTurnover3M = CurrentValues.Instance.AutoApproveMinTurnover3M,
				MinTurnover1Y = CurrentValues.Instance.AutoApproveMinTurnover1Y,
				MinMPSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays,
				MaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers,
				MaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans,
				MaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals,
				MaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate,
				MaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans,
				MinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion,
				MinAmount = CurrentValues.Instance.AutoApproveMinAmount,
				MaxAmount = CurrentValues.Instance.AutoApproveMaxAmount,
				IsSilent = CurrentValues.Instance.AutoApproveIsSilent,
				SilentTemplateName = CurrentValues.Instance.AutoApproveSilentTemplateName,
				SilentToAddress = CurrentValues.Instance.AutoApproveSilentToAddress,
				BusinessScoreThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold,
				AllowedCaisStatusesWithLoan = CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithLoan,
				AllowedCaisStatusesWithoutLoan = CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithoutLoan,
			});

			m_oTrail.MyInputData.SetArgs(customerId, autoApprovedAmount);

			m_oTrail.MyInputData.SetMetaData(new MetaData {
				RowType = "MetaData",
				IsBrokerCustomer = isBrokerCustomer,
				NumOfTodayAutoApproval = CalculateTodaysApprovals(),
				TodayLoanSum = CalculateTodaysLoans(),
				/* TODO
				FraudStatusValue =,
				AmlResult =,
				CustomerStatusName =,
				CustomerStatusEnabled =,
				CompanyScore =,
				ConsumerScore =,
				IncorporationDate =,
				DateOfBirth =,

				NumOfDefaultAccounts =,
				NumOfRollovers =,
				*/

				TotalLoanCount = loanRepository.ByCustomer(customerId).Count(),
				/*
				OfferValidUntil =,
				OfferStart =,
				EmailSendingBanned =,
				*/
			});

			m_oTrail.MyInputData.MetaData.Validate();

			m_oTrail.MyInputData.SetWorstStatuses(consumerCaisDetailWorstStatuses);
			FindLatePayments();
			m_oTrail.MyInputData.SetSeniority(CalculateSeniority());
			m_oTrail.MyInputData.SetAvailableFunds(availFunds.AvailableFunds, availFunds.ReservedAmount);
		} // SaveTrailInputData

		private void CheckAutoApprovalConformance(decimal outstandingOffers) {
			int nAutoApprovedAmount = this.autoApprovedAmount;

			if (nAutoApprovedAmount > 0)
				StepDone<InitialAssignment>().Init(this.autoApprovedAmount);
			else
				StepFailed<InitialAssignment>().Init(this.autoApprovedAmount);

			log.Debug("Checking if auto approval should take place for customer {0}...", customerId);

			try {
				CheckIsFraud();
				CheckIsBroker();
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


				StepDone<TotalLoanCount>().Init(m_oTrail.MyInputData.MetaData.TotalLoanCount);

				CheckWorstCaisStatus(
					m_oTrail.MyInputData.MetaData.TotalLoanCount > 0
					? CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithLoan
					: CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithoutLoan
				);

				CheckRollovers();
				CheckLateDays();
				decimal outstandingPrincipal = CheckOutstandingLoans();

				// Reduce the system calculated amount by the already open amount
				autoApprovedAmount -= (int)outstandingPrincipal;

				StepDone<ReduceOutstandingPrincipal>().Init(outstandingPrincipal, autoApprovedAmount);

				int autoApproveMinAmount = CurrentValues.Instance.AutoApproveMinAmount;
				int autoApproveMaxAmount = CurrentValues.Instance.AutoApproveMaxAmount;

				if (autoApprovedAmount < autoApproveMinAmount || autoApprovedAmount > autoApproveMaxAmount)
					StepFailed<AmountOutOfRangle>().Init(autoApprovedAmount, autoApproveMinAmount, autoApproveMaxAmount);
				else
					StepDone<AmountOutOfRangle>().Init(autoApprovedAmount, autoApproveMinAmount, autoApproveMaxAmount);

				nAutoApprovedAmount = autoApprovedAmount;

				if (nAutoApprovedAmount > 0)
					StepDone<Complete>().Init(nAutoApprovedAmount);
				else
					StepFailed<Complete>().Init(nAutoApprovedAmount);
			}
			catch (Exception ex) {
				StepFailed<ExceptionThrown>().Init(ex);
			} // try

			log.Debug("Checking if auto approval should take place for customer {0} complete.", customerId);

			log.Msg("Auto approved amount: {0}. {1}", autoApprovedAmount, m_oTrail);
		} // CheckAutoApprovalConformance

		private void CheckIsBroker() {
			if (isBrokerCustomer)
				StepFailed<IsBrokerCustomer>().Init();
			else
				StepDone<IsBrokerCustomer>().Init();
		} // CheckIsBroker

		private void CheckDefaultAccounts() {
			if (experianDefaultAccountRepository.GetAll().Any(entry => entry.Customer.Id == customerId))
				StepFailed<DefaultAccounts>().Init();
			else
				StepDone<DefaultAccounts>().Init();
		} // CheckDefaultAccounts

		private void CheckIsFraud() {
			if (customer == null)
				StepFailed<FraudSuspect>().Init(FraudStatus.UnderInvestigation);
			else if (customer.FraudStatus == 0)
				StepDone<FraudSuspect>().Init(customer.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(customer.FraudStatus);
		} // CheckIsFraud

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

			if (minCompanyScore <= 0)
				StepDone<BusinessScore>().Init(minCompanyScore, nThreshold);
			else if (minCompanyScore < nThreshold)
				StepFailed<BusinessScore>().Init(minCompanyScore, nThreshold);
			else
				StepDone<BusinessScore>().Init(minCompanyScore, nThreshold);
		} // CheckBusinessScore

		private void CheckExperianScore() {
			int autoApproveExperianScoreThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold;

			if (minExperianScore < autoApproveExperianScoreThreshold)
				StepFailed<ConsumerScore>().Init(minExperianScore, autoApproveExperianScoreThreshold);
			else
				StepDone<ConsumerScore>().Init(minExperianScore, autoApproveExperianScoreThreshold);
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

		private void CalculateTurnovers() {
			m_oTurnover.Clear();

			Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>> mpAnalysis = strategyHelper.GetAnalysisValsForCustomer(customerId);

			CalcOneTurnover(mpAnalysis, TimePeriodEnum.Month);
			CalcOneTurnover(mpAnalysis, TimePeriodEnum.Month3);
			CalcOneTurnover(mpAnalysis, TimePeriodEnum.Year);
		} // CalculateTurnovers

		private void CalcOneTurnover(Dictionary<MP_CustomerMarketPlace, List<IAnalysisDataParameterInfo>> mpAnalysis, TimePeriodEnum nPeriod) {
			m_oTurnover[nPeriod] = (decimal)strategyHelper.GetTurnoverForPeriod(mpAnalysis, nPeriod);
		} // CalcOneTurnover

		private void CheckTurnovers() {
			CheckOnePeriodTurnover(CurrentValues.Instance.AutoApproveMinTurnover1M, TimePeriodEnum.Month);
			CheckOnePeriodTurnover(CurrentValues.Instance.AutoApproveMinTurnover3M, TimePeriodEnum.Month3);
			CheckOnePeriodTurnover(CurrentValues.Instance.AutoApproveMinTurnover1Y, TimePeriodEnum.Year);
		} // CheckTurnovers

		private void CheckOnePeriodTurnover(
			int nThreshold,
			TimePeriodEnum nPeriod
		) {
			int turnover = (int)m_oTurnover[nPeriod];

			AThresholdTrace oTrace;

			switch (nPeriod) {
			case TimePeriodEnum.Month:
				oTrace = (turnover > nThreshold) ? StepDone<OneMonthTurnover>() : StepFailed<OneMonthTurnover>();
				break;

			case TimePeriodEnum.Month3:
				oTrace = (turnover > nThreshold) ? StepDone<ThreeMonthsTurnover>() : StepFailed<ThreeMonthsTurnover>();
				break;

			case TimePeriodEnum.Year:
				oTrace = (turnover > nThreshold) ? StepDone<OneYearTurnover>() : StepFailed<OneYearTurnover>();
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch

			oTrace.Init(turnover, nThreshold);
		} // CheckOnePeriodTurnover

		private int CalculateSeniority() {
			return customer == null ? -1 : strategyHelper.MarketplaceSeniority(customer);
		} // CalculateSeniority

		private void CheckSeniority() {
			int autoApproveMinMpSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays;

			if (m_oTrail.MyInputData.MarketplaceSeniority < autoApproveMinMpSeniorityDays)
				StepFailed<MarketplaceSeniority>().Init(m_oTrail.MyInputData.MarketplaceSeniority, autoApproveMinMpSeniorityDays);
			else
				StepDone<MarketplaceSeniority>().Init(m_oTrail.MyInputData.MarketplaceSeniority, autoApproveMinMpSeniorityDays);
		} // CheckSeniority

		private void CheckOutstandingOffers(decimal outstandingOffers) {
			int autoApproveMaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers;

			if (outstandingOffers >= autoApproveMaxOutstandingOffers)
				StepFailed<OutstandingOffers>().Init(outstandingOffers, autoApproveMaxOutstandingOffers);
			else
				StepDone<OutstandingOffers>().Init(outstandingOffers, autoApproveMaxOutstandingOffers);
		} // CheckOutstandingOffers

		private decimal CalculateTodaysLoans() {
			DateTime today = DateTime.UtcNow;

			var todayLoans = loanRepository.GetAll().Where(l => l.Date.Year == today.Year && l.Date.Month == today.Month && l.Date.Day == today.Day);

			decimal todayLoansAmount = 0;

			if (todayLoans.Any())
				todayLoansAmount = todayLoans.Sum(l => l.LoanAmount);

			return todayLoansAmount;
		} // CalculateTodaysLoans

		private void CheckTodaysLoans() {
			int autoApproveMaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans;

			if (m_oTrail.MyInputData.MetaData.TodayLoanSum >= autoApproveMaxTodayLoans)
				StepFailed<TodayLoans>().Init(m_oTrail.MyInputData.MetaData.TodayLoanSum, autoApproveMaxTodayLoans);
			else
				StepDone<TodayLoans>().Init(m_oTrail.MyInputData.MetaData.TodayLoanSum, autoApproveMaxTodayLoans);
		} // CheckTodaysLoans

		private int CalculateTodaysApprovals() {
			DateTime today = DateTime.UtcNow;
			return cashRequestsRepository.GetAll().Count(cr => cr.CreationDate.HasValue && cr.CreationDate.Value.Year == today.Year && cr.CreationDate.Value.Month == today.Month && cr.CreationDate.Value.Day == today.Day && cr.UnderwriterComment == "Auto Approval");
		} // CalculateTodaysApprovals

		private void CheckTodaysApprovals() {
			int autoApproveMaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals;

			if (m_oTrail.MyInputData.MetaData.NumOfTodayAutoApproval >= autoApproveMaxDailyApprovals)
				StepFailed<TodayApprovalCount>().Init(m_oTrail.MyInputData.MetaData.NumOfTodayAutoApproval, autoApproveMaxDailyApprovals);
			else
				StepDone<TodayApprovalCount>().Init(m_oTrail.MyInputData.MetaData.NumOfTodayAutoApproval, autoApproveMaxDailyApprovals);
		} // CheckTodaysApprovals

		private void CheckRollovers() {
			if (loanRepository.ByCustomer(customerId).Any(l => l.Schedule.Any(s => s.Rollovers.Any())))
				StepFailed<Rollovers>().Init();
			else
				StepDone<Rollovers>().Init();
		} // CheckRollovers

		private void CheckLateDays() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			if (m_oTrail.MyInputData.LatePayments.Count < 1) {
				StepDone<LatePayment>().Init(0, 0, DateTime.UtcNow, 0, DateTime.UtcNow, autoApproveMaxAllowedDaysLate);
				return;
			} // if

			foreach (Payment oPayment in m_oTrail.MyInputData.LatePayments) {
				StepFailed<LatePayment>().Init(
					oPayment.LoanID,
					oPayment.ScheduleID, oPayment.ScheduleDate,
					oPayment.TransactionID, oPayment.ScheduleDate,
					autoApproveMaxAllowedDaysLate
				);
			} // for
		} // CheckLateDays

		private void FindLatePayments() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			List<int> customerLoanIds = loanRepository.ByCustomer(customerId).Select(d => d.Id).ToList();

			foreach (int loanId in customerLoanIds) {
				int innerLoanId = loanId;

				var backfilledMapping = loanScheduleTransactionRepository.GetAll().Where(x => x.Loan.Id == innerLoanId);

				foreach (var paymentMapping in backfilledMapping) {
					var scheduleDate = new DateTime(paymentMapping.Schedule.Date.Year, paymentMapping.Schedule.Date.Month, paymentMapping.Schedule.Date.Day);

					var transactionDate = new DateTime(paymentMapping.Transaction.PostDate.Year, paymentMapping.Transaction.PostDate.Month, paymentMapping.Transaction.PostDate.Day);

					double nTotalLateDays = transactionDate.Subtract(scheduleDate).TotalDays;

					if (nTotalLateDays > autoApproveMaxAllowedDaysLate) {
						m_oTrail.MyInputData.AddLatePayment(new Payment {
							LoanID = innerLoanId,
							ScheduleDate = paymentMapping.Schedule.Date,
							ScheduleID = paymentMapping.Schedule.Id,
							TransactionID = paymentMapping.Transaction.Id,
							TransactionTime = paymentMapping.Transaction.PostDate,
						});
					} // if
				} // for
			} // for
		} // FindLatePayments

		private void FindOutstandingLoans() {
			/*
			MetaData oMeta = m_oTrail.MyInputData.MetaData; // just a shortcut

				TakenLoanAmount =,
				RepaidPrincipal =,
				SetupFees =,

			List<Loan> outstandingLoans = strategyHelper.GetOutstandingLoans(customerId);

			oMeta.OpenLoanCount = outstandingLoans.Count;
			oMeta.TakenLoanAmount = 0;
			oMeta.RepaidPrincipal = 0;
			oMeta.SetupFees = 0;

			foreach (var loan in outstandingLoans) {
				loanAmount += loan.LoanAmount;
				outstandingPrincipal += loan.Principal;
			} // for
			*/

		} // FindOutstandingLoans

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

		private void CheckWorstCaisStatus(string allowedStatuses) {
			List<string> oAllowedStatuses = allowedStatuses.Split(',').ToList();

			List<string> diff = consumerCaisDetailWorstStatuses.Except(oAllowedStatuses).ToList();

			if (diff.Count > 1)
				StepFailed<WorstCaisStatus>().Init(diff, consumerCaisDetailWorstStatuses, oAllowedStatuses);
			else
				StepDone<WorstCaisStatus>().Init(null, consumerCaisDetailWorstStatuses, oAllowedStatuses);
		} // CheckWorstCaisStatus

		private T StepFailed<T>() where T : ATrace {
			autoApprovedAmount = 0;
			return m_oTrail.Negative<T>(false);
		} // StepFailed

		private T StepDone<T>() where T : ATrace {
			return m_oTrail.Affirmative<T>(false);
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
		private readonly bool hasLoans;
		private readonly MedalClassification medalClassification;

		private int autoApprovedAmount;

		private readonly ApprovalTrail m_oTrail;

		private readonly SortedDictionary<TimePeriodEnum, decimal> m_oTurnover;

		private readonly AutomationCalculator.AutoDecision.AutoApproval.Agent m_oSecondaryImplementation;

		private readonly ASafeLog log;
	} // class Approval
} // namespace
