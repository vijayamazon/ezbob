﻿namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Web;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.Backend.Strategies.Experian;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Misc;
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailApi;
	using OfferCalculation;
	using StructureMap;

	public class Approval {
		public Approval(
			int customerId,
			int offeredCreditLine,
			Medal medalClassification,
			AConnection db,
			ASafeLog log
		) {
			this.db = db;
			this.log = log ?? new SafeLog();

			loanRepository = ObjectFactory.GetInstance<LoanRepository>();
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
			cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
			loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();

			this.customerId = customerId;
			this.autoApprovedAmount = offeredCreditLine;
			this.medalClassification = medalClassification;

			this.consumerCaisDetailWorstStatuses = new List<string>();

			customer = _customers.ReallyTryGet(customerId);

			m_oTrail = new ApprovalTrail(customerId, this.log, CurrentValues.Instance.AutomationExplanationMailReciever, CurrentValues.Instance.MailSenderEmail, CurrentValues.Instance.MailSenderName);

			m_oSecondaryImplementation = new Agent(
				customerId,
				offeredCreditLine,
				(AutomationCalculator.Common.Medal)medalClassification,
				db,
				log
			);

			m_oTurnover = new CalculatedTurnover();
		} // constructor

		public Approval Init() {
			var stra = new LoadExperianConsumerData(customerId, null, null, db, log);
			stra.Execute();

			m_oConsumerData = stra.Result;
			
			if (customer == null) {
				this.isBrokerCustomer = false;
				this.hasLoans = false;
			}
			else {
				this.isBrokerCustomer = customer.Broker != null;
				this.hasLoans = customer.Loans.Any();
			} // if
			
			if (customer != null && customer.Company != null) {
				if(customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited && customer.Company.ExperianRefNum != "NotFound") {
					var limited = new LoadExperianLtd(customer.Company.ExperianRefNum, 0, db, log);
					limited.Execute();
					this.directors = new List<Name>();
					foreach (var row in limited.Result.Children)
					{
						if (row.GetType() == typeof (ExperianLtdDL72)) {
							var dataRow = (ExperianLtdDL72) row;
							directors.Add(new Name(dataRow.FirstName, dataRow.LastName));
						}
						if (row.GetType() == typeof (ExperianLtdDLB5)) {
							var dataRow = (ExperianLtdDLB5) row;
							directors.Add(new Name(dataRow.FirstName, dataRow.LastName));
						}
					} // for each
				}
			}

			this.hmrcNames = new List<string>();

			db.ForEachRowSafe((names, hren) => {
				string name = ApprovalInputData.AdjustCompanyName(names["BusinessName"]);
				if (name != string.Empty)
					this.hmrcNames.Add(name);
				return ActionResult.Continue;
			}, "GetHmrcBusinessNames", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

			SafeReader sr = db.GetFirst(
				"GetExperianMinMaxConsumerDirectorsScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (!sr.IsEmpty)
				this.minExperianScore = sr["MinExperianScore"];

			var oScore = new QueryParameter("CompanyScore") {
				Type = DbType.Int32,
				Direction = ParameterDirection.Output,
			};

			var oDate = new QueryParameter("IncorporationDate") {
				Type = DbType.DateTime2,
				Direction = ParameterDirection.Output,
			};

			db.ExecuteNonQuery(
				"GetCompanyScoreAndIncorporationDate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("TakeMinScore", true),
				oScore,
				oDate
			);

			int nScore;
			if (int.TryParse(oScore.SafeReturnedValue, out nScore))
				this.minCompanyScore = nScore;

			this.consumerCaisDetailWorstStatuses.Clear();
			var oWorstStatuses = new SortedSet<string>();

			if (m_oConsumerData.Cais != null)
				foreach (var c in m_oConsumerData.Cais)
					oWorstStatuses.Add(c.WorstStatus.Trim());

			this.consumerCaisDetailWorstStatuses.AddRange(oWorstStatuses);

			m_oSecondaryImplementation.Init();

			return this;
		} // Init

		public bool MakeAndVerifyDecision() {
			var availFunds = new GetAvailableFunds(db, log);
			availFunds.Execute();

			SaveTrailInputData(availFunds);

			CheckAutoApprovalConformance(availFunds.ReservedAmount);
			m_oSecondaryImplementation.MakeDecision();

			bool bSuccess = m_oTrail.EqualsTo(m_oSecondaryImplementation.Trail);

			if (bSuccess && m_oTrail.HasDecided) {
				if (autoApprovedAmount == m_oSecondaryImplementation.Result.ApprovedAmount) {
					m_oTrail.Affirmative<SameAmount>(false).Init(autoApprovedAmount);
					m_oSecondaryImplementation.Trail.Affirmative<SameAmount>(false).Init(m_oSecondaryImplementation.Result.ApprovedAmount);
				}
				else {
					m_oTrail.Negative<SameAmount>(false).Init(autoApprovedAmount);
					m_oSecondaryImplementation.Trail.Negative<SameAmount>(false).Init(m_oSecondaryImplementation.Result.ApprovedAmount);
					bSuccess = false;
				} // if
			} // if

			m_oTrail.Save(db, m_oSecondaryImplementation.Trail);

			return bSuccess;
		} // MakeAndVerifyDecision

		public void MakeDecision(AutoDecisionResponse response) {
			try {
				response.LoanOfferUnderwriterComment = "Checking auto approve...";

				bool bSuccess = MakeAndVerifyDecision();

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

					response.LoanOfferUnderwriterComment = "Mismatch - " + m_oTrail.DiffID;

					response.AutoApproveAmount = 0;

					response.CreditResult = CreditResultStatus.WaitingForDecision;
					response.UserStatus = Status.Manual;
					response.SystemDecision = SystemDecision.Manual;
				} // if

				decimal minLoanAmount = CurrentValues.Instance.MinLoanAmount;

				response.AutoApproveAmount = (int)(
					Math.Round(response.AutoApproveAmount / minLoanAmount, 0, MidpointRounding.AwayFromZero) * minLoanAmount
				);

				log.Info("Decided to auto approve rounded amount: {0}", response.AutoApproveAmount);

				if (response.AutoApproveAmount != 0) {
					if (m_oTrail.MyInputData.AvailableFunds > response.AutoApproveAmount) {
						if (CurrentValues.Instance.AutoApproveIsSilent) {
							NotifyAutoApproveSilentMode(response);

							response.LoanOfferUnderwriterComment = "Silent Approve - " + m_oTrail.DiffID;
							response.CreditResult = CreditResultStatus.WaitingForDecision;
							response.UserStatus = Status.Manual;
							response.SystemDecision = SystemDecision.Manual;
						}
						else
						{
							var offerDualCalculator = new OfferDualCalculator(db, log);
							OfferResult offerResult = offerDualCalculator.CalculateOffer(customerId, DateTime.UtcNow, response.AutoApproveAmount, hasLoans, medalClassification);
							if (offerResult == null || !string.IsNullOrEmpty(offerResult.Error))
							{
								log.Alert("Failed calculating offer for auto-approve error:{0}. Will use manual. Customer:{1}", offerResult != null ? offerResult.Error : "", customerId);
								response.CreditResult = CreditResultStatus.WaitingForDecision;
								response.UserStatus = Status.Manual;
								response.SystemDecision = SystemDecision.Manual;
								response.LoanOfferUnderwriterComment = "Calculator failure - " + m_oTrail.DiffID;
							}
							else
							{
								response.CreditResult = CreditResultStatus.Approved;
								response.UserStatus = Status.Approved;
								response.SystemDecision = SystemDecision.Approve;
								response.LoanOfferUnderwriterComment = "Auto Approval";
								response.DecisionName = "Approval";
								response.AppValidFor = DateTime.UtcNow.AddDays(m_oTrail.MyInputData.MetaData.OfferLength);
								response.Decision = DecisionActions.Approve;
								response.LoanOfferEmailSendingBannedNew = m_oTrail.MyInputData.MetaData.IsEmailSendingBanned;

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
				response.LoanOfferUnderwriterComment = "Exception - " + m_oTrail.DiffID;
			} // try
		} // MakeDecision

		private int FindNumOfDefaultAccounts() {
			if (m_oConsumerData.Cais.Count == 0)
				return 0;

			return m_oConsumerData.Cais.Count(c => c.AccountStatus == "F");
		} // FindNumOfDefaultAccounts

		private void SaveTrailInputData(GetAvailableFunds availFunds) {
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
				OnlineTurnoverAge = CurrentValues.Instance.AutoApproveOnlineTurnoverAge,
				OnlineTurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveOnlineTurnoverDropQuarterRatio,
				OnlineTurnoverDropMonthRatio = CurrentValues.Instance.AutoApproveOnlineTurnoverDropMonthRatio,
				HmrcTurnoverAge = CurrentValues.Instance.AutoApproveHmrcTurnoverAge,
				HmrcTurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveHmrcTurnoverDropQuarterRatio,
				HmrcTurnoverDropHalfYearRatio = CurrentValues.Instance.AutoApproveHmrcTurnoverDropHalfYearRatio,
			});

			m_oTrail.MyInputData.SetArgs(customerId, autoApprovedAmount, (AutomationCalculator.Common.Medal)medalClassification);

			m_oTrail.MyInputData.SetMetaData(new MetaData {
				RowType = "MetaData",

				FirstName = (customer != null) && (customer.PersonalInfo != null) ? customer.PersonalInfo.FirstName : null,
				LastName = (customer != null) && (customer.PersonalInfo != null) ? customer.PersonalInfo.Surname : null,

				IsBrokerCustomer = isBrokerCustomer,
				NumOfTodayAutoApproval = CalculateTodaysApprovals(),
				TodayLoanSum = CalculateTodaysLoans(),
				FraudStatusValue = (int)((customer == null) ? FraudStatus.UnderInvestigation : customer.FraudStatus),
				AmlResult = (customer == null) ? "failed because customer not found" : customer.AMLResult,
				CustomerStatusName = customer == null ? "unknown" : customer.CollectionStatus.CurrentStatus.Name,
				CustomerStatusEnabled = customer != null && customer.CollectionStatus.CurrentStatus.IsEnabled,
				CompanyScore = minCompanyScore,
				ConsumerScore = minExperianScore,
				IncorporationDate = strategyHelper.GetCustomerIncorporationDate(customer),
				DateOfBirth = ((customer != null) && (customer.PersonalInfo != null) && customer.PersonalInfo.DateOfBirth.HasValue) ? customer.PersonalInfo.DateOfBirth.Value : DateTime.UtcNow,

				NumOfDefaultAccounts = FindNumOfDefaultAccounts(),
				NumOfRollovers = CalculateRollovers(),

				TotalLoanCount = loanRepository.ByCustomer(customerId).Count(),

				ExperianCompanyName = (customer != null) && (customer.Company != null) ? customer.Company.ExperianCompanyName : null,
				EnteredCompanyName = (customer != null) && (customer.Company != null) ? customer.Company.CompanyName : null,
				IsLimitedCompanyType =  (this.customer != null) && (this.customer.PersonalInfo != null) && this.customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited
			});

			FindOutstandingLoans();

			SafeReader sr = db.GetFirst(
				"GetLastOfferDataForApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			m_oTrail.MyInputData.MetaData.EmailSendingBanned = sr["EmailSendingBanned"];
			m_oTrail.MyInputData.MetaData.OfferStart = sr["OfferStart"];
			m_oTrail.MyInputData.MetaData.OfferValidUntil = sr["OfferValidUntil"];

			m_oTrail.MyInputData.SetDirectorNames(this.directors);
			m_oTrail.MyInputData.SetHmrcBusinessNames(this.hmrcNames);

			m_oTrail.MyInputData.SetWorstStatuses(consumerCaisDetailWorstStatuses);
			FindLatePayments();
			m_oTrail.MyInputData.SetSeniority(CalculateSeniority());
			m_oTrail.MyInputData.SetAvailableFunds(availFunds.AvailableFunds, availFunds.ReservedAmount);

			foreach (int nMonthCount in ms_oTurnoverMonths) {
				this.db.ForEachRowSafe(
					r => m_oTurnover.Add(r, this.log),
					"GetCustomerTurnoverData",
					new QueryParameter("OnlineOnly", true),
					new QueryParameter("CustomerID", this.customerId),
					new QueryParameter("MonthCount", nMonthCount)
				);
			} // for each

			m_oTrail.MyInputData.SetTurnoverData(m_oTurnover);

			m_oTrail.MyInputData.MetaData.Validate();
		} // SaveTrailInputData

		private void CheckAutoApprovalConformance(decimal outstandingOffers) {
			log.Debug("Primary: checking if auto approval should take place for customer {0}...", customerId);

			try {
				CheckInit();

				CheckMedal();
				CheckIsFraud();
				CheckIsBroker();
				CheckTodaysApprovals();
				CheckTodaysLoans();
				CheckOutstandingOffers(outstandingOffers);
				CheckAMLResult();
				CheckCustomerStatus();
				CheckBusinessScore();
				CheckExperianScore();
				CheckAge();
				CheckOnlineTurnovers();
				CheckHmrcTurnovers();
				CheckSeniority();
				CheckDefaultAccounts();
				CheckIsDirector();
				CheckHmrcIsCompany();
				CheckTotalLoanCount();
				CheckWorstCaisStatus(
					m_oTrail.MyInputData.MetaData.TotalLoanCount > 0
					? CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithLoan
					: CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithoutLoan
				);
				CheckRollovers();
				CheckLateDays();
				CheckCustomerOpenLoans();
				CheckRepaidRatio();
				ReduceOutstandingPrincipal();

				CheckAllowedRange();

				CheckComplete();
			}
			catch (Exception ex) {
				StepFailed<ExceptionThrown>().Init(ex);
			} // try

			log.Debug("Primary: checking if auto approval should take place for customer {0} complete.", customerId);

			log.Msg("Primary: auto approved amount: {0}. {1}", autoApprovedAmount, m_oTrail);
		} // CheckAutoApprovalConformance

		private void CheckMedal() {
			if (medalClassification == Medal.NoClassification)
				StepFailed<MedalIsGood>().Init((AutomationCalculator.Common.Medal)medalClassification);
			else
				StepDone<MedalIsGood>().Init((AutomationCalculator.Common.Medal)medalClassification);
		} // CheckMedal

		private void CheckIsBroker() {
			if (isBrokerCustomer)
				StepFailed<IsBrokerCustomer>().Init();
			else
				StepDone<IsBrokerCustomer>().Init();
		} // CheckIsBroker

		private void CheckDefaultAccounts() {
			if (m_oTrail.MyInputData.MetaData.NumOfDefaultAccounts > 0)
				StepFailed<DefaultAccounts>().Init(m_oTrail.MyInputData.MetaData.NumOfDefaultAccounts);
			else
				StepDone<DefaultAccounts>().Init(m_oTrail.MyInputData.MetaData.NumOfDefaultAccounts);
		} // CheckDefaultAccounts

		private void CheckIsDirector() {
			if (!m_oTrail.MyInputData.MetaData.IsLimitedCompanyType) {
				StepDone<CustomerIsDirector>().Init(m_oTrail.MyInputData.MetaData.IsLimitedCompanyType);
				return;
			}

			bool isDirector = false;

			if (m_oTrail.MyInputData.DirectorNames.Count < 1) {
				StepFailed<CustomerIsDirector>().Init(
					m_oTrail.MyInputData.CustomerName.ToString() 
				);
				return;
			} // if

			foreach (Name directorName in m_oTrail.MyInputData.DirectorNames) {
				if (directorName.Equals(m_oTrail.MyInputData.CustomerName)) {
					isDirector = true;
					break;
				} // if
			} // for

			if (!isDirector) {
				StepFailed<CustomerIsDirector>().Init(
					m_oTrail.MyInputData.CustomerName.ToString(), 
					m_oTrail.MyInputData.DirectorNames.Select(x => x.ToString()).ToList()
				);
			}
			else {
				StepDone<CustomerIsDirector>().Init(
					m_oTrail.MyInputData.CustomerName.ToString(), 
					m_oTrail.MyInputData.DirectorNames.Select(x => x.ToString()).ToList()
				);
			} // if
		} // CheckIsDirector
		
		private void CheckHmrcIsCompany() {
			bool isCompany = false;

			if (m_oTrail.MyInputData.HmrcBusinessNames.Count < 1) {
				StepDone<HmrcIsOfBusiness>().Init();
				return;
			} // if

			foreach (string hmrcName in m_oTrail.MyInputData.HmrcBusinessNames) {
				if (hmrcName.Equals(m_oTrail.MyInputData.CompanyName)) {
					isCompany = true;
					break;
				} // if
			} // for

			if (!isCompany) {
				StepFailed<HmrcIsOfBusiness>().Init(
					m_oTrail.MyInputData.HmrcBusinessNames,
					m_oTrail.MyInputData.CompanyName
				);
			}
			else {
				StepDone<HmrcIsOfBusiness>().Init(
					m_oTrail.MyInputData.HmrcBusinessNames,
					m_oTrail.MyInputData.CompanyName
				);
			} // if
		} // CheckHmrcIsCompany

		private void CheckIsFraud() {
			if (m_oTrail.MyInputData.MetaData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(m_oTrail.MyInputData.MetaData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(m_oTrail.MyInputData.MetaData.FraudStatus);
		} // CheckIsFraud

		private void CheckAMLResult() {
			if (m_oTrail.MyInputData.MetaData.AmlResult != "Passed")
				StepFailed<AmlCheck>().Init(m_oTrail.MyInputData.MetaData.AmlResult);
			else
				StepDone<AmlCheck>().Init(m_oTrail.MyInputData.MetaData.AmlResult);
		} // CheckAMLResult

		private void CheckCustomerStatus() {
			if (!m_oTrail.MyInputData.MetaData.CustomerStatusEnabled)
				StepFailed<CustomerStatus>().Init(m_oTrail.MyInputData.MetaData.CustomerStatusName);
			else
				StepDone<CustomerStatus>().Init(m_oTrail.MyInputData.MetaData.CustomerStatusName);
		} // CheckCustomerStatus

		private void CheckBusinessScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold;
			int nScore = m_oTrail.MyInputData.MetaData.CompanyScore;

			if (nScore <= 0)
				StepDone<BusinessScore>().Init(nScore, nThreshold);
			else if (nScore < nThreshold)
				StepFailed<BusinessScore>().Init(nScore, nThreshold);
			else
				StepDone<BusinessScore>().Init(nScore, nThreshold);
		} // CheckBusinessScore

		private void CheckExperianScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold;
			int nScore = m_oTrail.MyInputData.MetaData.ConsumerScore;

			if (nScore < nThreshold)
				StepFailed<ConsumerScore>().Init(nScore, nThreshold);
			else
				StepDone<ConsumerScore>().Init(nScore, nThreshold);
		} // CheckExperianScore

		private void CheckAge() {
			int autoApproveCustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge;
			int autoApproveCustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge;

			if ((customer == null) || (customer.PersonalInfo == null) || (customer.PersonalInfo.DateOfBirth == null))
				StepFailed<Age>().Init(-1, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
			else {
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

		private int CalculateSeniority() {
			return customer == null ? -1 : strategyHelper.MarketplaceSeniority(customer);
		} // CalculateSeniority

		private void CheckOnlineTurnovers() {
			if (!m_oTrail.MyInputData.HasOnline && m_oTrail.MyInputData.HasHmrc) {
				StepDone<OnlineTurnoverAge>().Init(m_oTrail.MyInputData.HasOnline);
				return;
			} // if

			if (m_oTrail.MyInputData.IsOnlineTurnoverTooOld())
				StepFailed<OnlineTurnoverAge>().Init(m_oTrail.MyInputData.OnlineUpdateTime, m_oTrail.MyInputData.DataAsOf);
			else
				StepDone<OnlineTurnoverAge>().Init(m_oTrail.MyInputData.OnlineUpdateTime, m_oTrail.MyInputData.DataAsOf);

			if (m_oTrail.MyInputData.IsOnlineTurnoverGood(1))
				StepDone<OnlineOneMonthTurnover>().Init(m_oTrail.MyInputData.OnlineTurnover1M, m_oTrail.MyInputData.OnlineTurnover1Y, m_oTrail.MyInputData.Configuration.OnlineTurnoverDropMonthRatio);
			else
				StepFailed<OnlineOneMonthTurnover>().Init(m_oTrail.MyInputData.OnlineTurnover1M, m_oTrail.MyInputData.OnlineTurnover1Y, m_oTrail.MyInputData.Configuration.OnlineTurnoverDropMonthRatio);

			if (m_oTrail.MyInputData.IsOnlineTurnoverGood(3))
				StepDone<OnlineThreeMonthsTurnover>().Init(m_oTrail.MyInputData.OnlineTurnover3M, m_oTrail.MyInputData.OnlineTurnover1Y, m_oTrail.MyInputData.Configuration.OnlineTurnoverDropQuarterRatio);
			else
				StepFailed<OnlineThreeMonthsTurnover>().Init(m_oTrail.MyInputData.OnlineTurnover3M, m_oTrail.MyInputData.OnlineTurnover1Y, m_oTrail.MyInputData.Configuration.OnlineTurnoverDropQuarterRatio);
		} // CheckOnlineTurnovers

		private void CheckHmrcTurnovers() {
			if (!m_oTrail.MyInputData.HasHmrc && m_oTrail.MyInputData.HasOnline) {
				StepDone<HmrcTurnoverAge>().Init(m_oTrail.MyInputData.HasHmrc);
				return;
			} // if

			if (m_oTrail.MyInputData.IsHmrcTurnoverTooOld())
				StepFailed<HmrcTurnoverAge>().Init(m_oTrail.MyInputData.HmrcUpdateTime, m_oTrail.MyInputData.DataAsOf);
			else
				StepDone<HmrcTurnoverAge>().Init(m_oTrail.MyInputData.HmrcUpdateTime, m_oTrail.MyInputData.DataAsOf);

			if (m_oTrail.MyInputData.IsHmrcTurnoverGood(3))
				StepDone<HmrcThreeMonthsTurnover>().Init(m_oTrail.MyInputData.HmrcTurnover3M, m_oTrail.MyInputData.HmrcTurnover1Y, m_oTrail.MyInputData.Configuration.HmrcTurnoverDropQuarterRatio);
			else
				StepFailed<HmrcThreeMonthsTurnover>().Init(m_oTrail.MyInputData.HmrcTurnover3M, m_oTrail.MyInputData.HmrcTurnover1Y, m_oTrail.MyInputData.Configuration.HmrcTurnoverDropQuarterRatio);

			if (m_oTrail.MyInputData.IsHmrcTurnoverGood(6))
				StepDone<HalfYearTurnover>().Init(m_oTrail.MyInputData.HmrcTurnover6M, m_oTrail.MyInputData.HmrcTurnover1Y, m_oTrail.MyInputData.Configuration.HmrcTurnoverDropHalfYearRatio);
			else
				StepFailed<HalfYearTurnover>().Init(m_oTrail.MyInputData.HmrcTurnover6M, m_oTrail.MyInputData.HmrcTurnover1Y, m_oTrail.MyInputData.Configuration.HmrcTurnoverDropHalfYearRatio);
		} // CheckHmrcTurnovers

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

		private int CalculateRollovers() {
			return loanRepository.ByCustomer(customerId).SelectMany(loan => loan.Schedule).Sum(sch => sch.Rollovers.Count());
		} // CalculateRollovers

		private void CheckRollovers() {
			if (m_oTrail.MyInputData.MetaData.NumOfRollovers > 0)
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
					oPayment.TransactionID, oPayment.TransactionTime,
					autoApproveMaxAllowedDaysLate
				);
			} // for
		} // CheckLateDays

		private void FindLatePayments() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			List<int> customerLoanIds = loanRepository.ByCustomer(customerId).Select(d => d.Id).ToList();

			foreach (int loanId in customerLoanIds) {
				int innerLoanId = loanId;

				IQueryable<LoanScheduleTransaction> backfilledMapping =
					loanScheduleTransactionRepository
					.GetAll()
					.Where(x =>
						x.Loan.Id == innerLoanId &&
						x.Schedule.Date.Date < x.Transaction.PostDate.Date &&
						x.Transaction.Status == LoanTransactionStatus.Done
					);

				foreach (var paymentMapping in backfilledMapping) {
					DateTime scheduleDate = paymentMapping.Schedule.Date.Date;

					DateTime transactionDate = paymentMapping.Transaction.PostDate.Date;

					double nTotalLateDays = (transactionDate - scheduleDate).TotalDays;

					if (nTotalLateDays > autoApproveMaxAllowedDaysLate) {
						m_oTrail.MyInputData.AddLatePayment(new Payment {
							LoanID = innerLoanId,
							ScheduleDate = paymentMapping.Schedule.Date.Date,
							ScheduleID = paymentMapping.Schedule.Id,
							TransactionID = paymentMapping.Transaction.Id,
							TransactionTime = paymentMapping.Transaction.PostDate,
						});
					} // if
				} // for
			} // for
		} // FindLatePayments

		private void FindOutstandingLoans() {
			MetaData oMeta = m_oTrail.MyInputData.MetaData; // just a shortcut

			List<Loan> outstandingLoans = strategyHelper.GetOutstandingLoans(customerId);

			oMeta.OpenLoanCount = outstandingLoans.Count;
			oMeta.TakenLoanAmount = 0;
			oMeta.RepaidPrincipal = 0;
			oMeta.SetupFees = 0;

			foreach (var loan in outstandingLoans) {
				oMeta.TakenLoanAmount += loan.LoanAmount;
				oMeta.RepaidPrincipal += loan.LoanAmount - loan.Principal;
				oMeta.SetupFees += loan.SetupFee;
			} // for
		} // FindOutstandingLoans

		private void CheckCustomerOpenLoans() {
			int autoApproveMaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans;

			if (m_oTrail.MyInputData.MetaData.OpenLoanCount > autoApproveMaxNumOfOutstandingLoans)
				StepFailed<OutstandingLoanCount>().Init(m_oTrail.MyInputData.MetaData.OpenLoanCount, autoApproveMaxNumOfOutstandingLoans);
			else
				StepDone<OutstandingLoanCount>().Init(m_oTrail.MyInputData.MetaData.OpenLoanCount, autoApproveMaxNumOfOutstandingLoans);
		} // CheckCustomerOpenLoans

		private void CheckRepaidRatio() {
			decimal autoApproveMinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion;

			if (m_oTrail.MyInputData.MetaData.RepaidRatio >= autoApproveMinRepaidPortion)
				StepDone<OutstandingRepayRatio>().Init(m_oTrail.MyInputData.MetaData.RepaidRatio, autoApproveMinRepaidPortion);
			else
				StepFailed<OutstandingRepayRatio>().Init(m_oTrail.MyInputData.MetaData.RepaidRatio, autoApproveMinRepaidPortion);
		} // CheckRepaidRatio

		private void ReduceOutstandingPrincipal() {
			autoApprovedAmount -= (int)m_oTrail.MyInputData.MetaData.OutstandingPrincipal;

			if (autoApprovedAmount < 0)
				autoApprovedAmount = 0;

			if (autoApprovedAmount > 0.00000001m)
				StepDone<ReduceOutstandingPrincipal>().Init(m_oTrail.MyInputData.MetaData.OutstandingPrincipal, autoApprovedAmount);
			else
				StepFailed<ReduceOutstandingPrincipal>().Init(m_oTrail.MyInputData.MetaData.OutstandingPrincipal, autoApprovedAmount);
		} // ReduceOutstandingPrincipal

		private void CheckAllowedRange() {
			if (m_oTrail.MyInputData.Configuration.IsSilent) {
				StepDone<AmountOutOfRangle>().Init(autoApprovedAmount, m_oTrail.MyInputData.Configuration.IsSilent);
			}
			else {
				int autoApproveMinAmount = m_oTrail.MyInputData.Configuration.MinAmount;
				int autoApproveMaxAmount = m_oTrail.MyInputData.Configuration.MaxAmount;

				if (autoApprovedAmount < autoApproveMinAmount || autoApprovedAmount > autoApproveMaxAmount)
					StepFailed<AmountOutOfRangle>().Init(autoApprovedAmount, autoApproveMinAmount, autoApproveMaxAmount);
				else
					StepDone<AmountOutOfRangle>().Init(autoApprovedAmount, autoApproveMinAmount, autoApproveMaxAmount);
			}
		} // CheckAllowedRange

		private void CheckWorstCaisStatus(string allowedStatuses) {
			List<string> oAllowedStatuses = allowedStatuses.Split(',').ToList();

			List<string> diff = consumerCaisDetailWorstStatuses.Except(oAllowedStatuses).ToList();

			if (diff.Count > 1)
				StepFailed<WorstCaisStatus>().Init(diff, consumerCaisDetailWorstStatuses, oAllowedStatuses);
			else
				StepDone<WorstCaisStatus>().Init(null, consumerCaisDetailWorstStatuses, oAllowedStatuses);
		} // CheckWorstCaisStatus

		private void CheckComplete() {
			int nAutoApprovedAmount = autoApprovedAmount;

			if (nAutoApprovedAmount > 0)
				StepDone<Complete>().Init(nAutoApprovedAmount);
			else
				StepFailed<Complete>().Init(nAutoApprovedAmount);
		} // CheckComplete

		private void CheckInit() {
			int nAutoApprovedAmount = this.autoApprovedAmount;

			if (nAutoApprovedAmount > 0)
				StepDone<InitialAssignment>().Init(this.autoApprovedAmount);
			else
				StepFailed<InitialAssignment>().Init(this.autoApprovedAmount);
		} // CheckInit

		private void CheckTotalLoanCount() {
			StepDone<TotalLoanCount>().Init(m_oTrail.MyInputData.MetaData.TotalLoanCount);
		} // CheckTotalLoanCount

		private T StepFailed<T>() where T : ATrace {
			autoApprovedAmount = 0;
			return m_oTrail.Negative<T>(false);
		} // StepFailed

		private T StepDone<T>() where T : ATrace {
			return m_oTrail.Affirmative<T>(false);
		} // StepFailed

		private void NotifyAutoApproveSilentMode(AutoDecisionResponse data) {
			try {
				var message = string.Format(
					@"<h1><u>Silent auto approve for customer <b style='color:red'>{0}</b></u></h1><br>
					<h2><b>Offer:</b></h2>
					<pre><h3>Amount: {1}</h3></pre><br>
					<pre><h3>Period: {2}</h3></pre><br>
					<pre><h3>Interest rate: {3}</h3></pre><br>
					<pre><h3>Setup fee: {4}</h3></pre><br>
					<h2><b>Decision flow:</b></h2>
					<pre><h3>{5}</h3></pre><br>
					<h2><b>Decision data:</b></h2>
					<pre><h3>{6}</h3></pre>",
					customerId,
					data.AutoApproveAmount,
					data.RepaymentPeriod,
					data.InterestRate,
					data.SetupFee,
					HttpUtility.HtmlEncode(m_oTrail.ToString()),
					HttpUtility.HtmlEncode(m_oTrail.InputData.Serialize())
				);

				new Mail().Send(
					CurrentValues.Instance.AutoApproveSilentToAddress,
					null,
					message,
					CurrentValues.Instance.MailSenderEmail,
					CurrentValues.Instance.MailSenderName,
					"#SilentApprove for customer " + customerId
				);
			}
			catch (Exception e) {
				log.Error(e, "Failed sending alert mail - silent auto approval.");
			} // try
		} // NotifyAutoApproveSilentMode

		private static readonly SortedSet<int> ms_oTurnoverMonths = new SortedSet<int> { 1, 3, 6, 12, };

		private readonly CalculatedTurnover m_oTurnover;

		private readonly CustomerRepository _customers;
		private readonly CashRequestsRepository cashRequestsRepository;
		private readonly LoanScheduleTransactionRepository loanScheduleTransactionRepository;
		private readonly LoanRepository loanRepository;

		private readonly AConnection db;
		private readonly Customer customer;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();

		private bool isBrokerCustomer;
		private int minExperianScore;
		private int minCompanyScore;
		private bool hasLoans;

		private ExperianConsumerData m_oConsumerData;

		private readonly List<string> consumerCaisDetailWorstStatuses;
		private readonly int customerId;
		private readonly Medal medalClassification;

		private int autoApprovedAmount;

		private readonly ApprovalTrail m_oTrail;

		private readonly AutomationCalculator.AutoDecision.AutoApproval.Agent m_oSecondaryImplementation;

		private readonly ASafeLog log;
		private List<Name> directors;
		private List<String> hmrcNames;
	} // class Approval
} // namespace
