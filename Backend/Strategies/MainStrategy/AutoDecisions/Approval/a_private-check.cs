namespace Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.Approval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using AutomationCalculator.ProcessHistory.Common;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;

	public partial class Approval {
		private void CheckAutoApprovalConformance(decimal outstandingOffers) {
			this.log.Debug("Primary: checking if auto approval should take place for customer {0}...", this.customerId);

			try {
				CheckInit();

				CheckMedal();
				CheckIsFraud();
				CheckIsBroker();
				CheckCompanyIsDissolved();
				CheckTodaysApprovals();
				CheckTodaysLoans();
				CheckOutstandingOffers(outstandingOffers);
				CheckAMLResult();
				CheckCustomerStatus();
				CheckBusinessScore();
				CheckExperianScore();
				CheckAge();
				CheckTurnovers();
				CheckSeniority();
				CheckDefaultAccounts();
				CheckIsDirector();
				CheckHmrcIsCompany();
				CheckTotalLoanCount();
				CheckWorstCaisStatus(this.m_oTrail.MyInputData.MetaData.TotalLoanCount > 0
					? CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithLoan
					: CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithoutLoan
				);
				CheckRollovers();
				CheckLateDays();
				CheckCustomerOpenLoans();
				CheckRepaidRatio();
				ReduceOutstandingPrincipal();

				CheckAllowedRange();

				decimal roundTo = CurrentValues.Instance.GetCashSliderStep;

				if (roundTo < 0.00000001m)
					roundTo = 1m;

				this.log.Debug(
					"Primary before rounding: amount = {0}, minLoanAmount = {1}",
					this.m_oTrail.SafeAmount,
					roundTo
				);

				this.m_oTrail.Amount = roundTo * Math.Round(
					this.m_oTrail.SafeAmount / roundTo, 0, MidpointRounding.AwayFromZero
				);

				this.log.Debug(
					"Primary after rounding: amount = {0}, minLoanAmount = {1}",
					this.m_oTrail.SafeAmount,
					roundTo
				);

				CheckComplete();
			} catch (Exception ex) {
				StepForceFailed<ExceptionThrown>().Init(ex);
			} // try

			this.log.Debug(
				"Primary: checking if auto approval should take place for customer {0} complete.",
				this.customerId
			);

			this.log.Msg("Primary: auto approved amount: {0}. {1}", this.m_oTrail.RoundedAmount, this.m_oTrail);
		} // CheckAutoApprovalConformance

		private void CheckCompanyIsDissolved() {
			bool isDissolved =
				m_oTrail.MyInputData.MetaData.CompanyDissolutionDate.HasValue &&
				m_oTrail.MyInputData.MetaData.CompanyDissolutionDate.Value <= Now;

			if (isDissolved)
				StepFailed<CompanyIsDissolved>().Init(m_oTrail.MyInputData.MetaData.CompanyDissolutionDate);
			else
				StepDone<CompanyIsDissolved>().Init(m_oTrail.MyInputData.MetaData.CompanyDissolutionDate);
		} // CheckCompanyIsDissolved

		private void CheckAge() {
			int autoApproveCustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge;
			int autoApproveCustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge;

			bool hasNoBirthdate = 
				(this.customer == null) ||
				(this.customer.PersonalInfo == null) ||
				(this.customer.PersonalInfo.DateOfBirth == null);

			if (hasNoBirthdate) {
				StepFailed<Age>().Init(-1, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
			} else {
				DateTime now = Now;

				int customerAge = now.Year - this.customer.PersonalInfo.DateOfBirth.Value.Year;

				if (now < this.customer.PersonalInfo.DateOfBirth.Value.AddYears(customerAge))
					customerAge--;

				if (customerAge < autoApproveCustomerMinAge || customerAge > autoApproveCustomerMaxAge)
					StepFailed<Age>().Init(customerAge, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
				else
					StepDone<Age>().Init(customerAge, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
			} // if
		} // CheckAge

		private void CheckAllowedRange() {
			if (this.m_oTrail.MyInputData.Configuration.IsSilent) {
				StepDone<AmountOutOfRangle>()
					.Init(this.m_oTrail.RoundedAmount, this.m_oTrail.MyInputData.Configuration.IsSilent);
			} else {
				int autoApproveMinAmount = this.m_oTrail.MyInputData.Configuration.MinLoan;
				int autoApproveMaxAmount = this.m_oTrail.MyInputData.Configuration.MaxAmount;

				if (this.m_oTrail.RoundedAmount < autoApproveMinAmount || this.m_oTrail.RoundedAmount > autoApproveMaxAmount) {
					StepFailed<AmountOutOfRangle>().Init(
						this.m_oTrail.RoundedAmount,
						autoApproveMinAmount,
						autoApproveMaxAmount
					);
				} else {
					StepDone<AmountOutOfRangle>().Init(
						this.m_oTrail.RoundedAmount,
						autoApproveMinAmount,
						autoApproveMaxAmount
					);
				}
			} // if
		} // CheckAllowedRange

		private void CheckAMLResult() {
			bool amlPassed =
				(this.m_oTrail.MyInputData.MetaData.PreviousManualApproveCount > 0) ||
				(this.m_oTrail.MyInputData.MetaData.AmlResult == "Passed");

			if (amlPassed)
				StepDone<AmlCheck>().Init(this.m_oTrail.MyInputData.MetaData.AmlResult);
			else
				StepFailed<AmlCheck>().Init(this.m_oTrail.MyInputData.MetaData.AmlResult);
		} // CheckAMLResult

		private void CheckBusinessScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold;
			int nScore = this.m_oTrail.MyInputData.MetaData.CompanyScore;

			if (nScore <= 0)
				StepDone<BusinessScore>().Init(nScore, nThreshold);
			else if (nScore < nThreshold)
				StepFailed<BusinessScore>().Init(nScore, nThreshold);
			else
				StepDone<BusinessScore>().Init(nScore, nThreshold);
		} // CheckBusinessScore

		private void CheckComplete() {
			int nAutoApprovedAmount = this.m_oTrail.RoundedAmount;

			if (nAutoApprovedAmount > 0)
				StepDone<Complete>().Init(nAutoApprovedAmount);
			else
				StepFailed<Complete>().Init(nAutoApprovedAmount);
		} // CheckComplete

		private void CheckCustomerOpenLoans() {
			int autoApproveMaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans;

			if (this.m_oTrail.MyInputData.MetaData.OpenLoanCount > autoApproveMaxNumOfOutstandingLoans) {
				StepFailed<OutstandingLoanCount>().Init(
					this.m_oTrail.MyInputData.MetaData.OpenLoanCount,
					autoApproveMaxNumOfOutstandingLoans
				);
			} else {
				StepDone<OutstandingLoanCount>().Init(
					this.m_oTrail.MyInputData.MetaData.OpenLoanCount,
					autoApproveMaxNumOfOutstandingLoans
				);
			} // if
		} // CheckCustomerOpenLoans

		private void CheckCustomerStatus() {
			if (!this.m_oTrail.MyInputData.MetaData.CustomerStatusEnabled)
				StepFailed<CustomerStatus>().Init(this.m_oTrail.MyInputData.MetaData.CustomerStatusName);
			else
				StepDone<CustomerStatus>().Init(this.m_oTrail.MyInputData.MetaData.CustomerStatusName);
		} // CheckCustomerStatus

		private void CheckDefaultAccounts() {
			if (this.m_oTrail.MyInputData.MetaData.NumOfDefaultAccounts > 0)
				StepFailed<DefaultAccounts>().Init(this.m_oTrail.MyInputData.MetaData.NumOfDefaultAccounts);
			else
				StepDone<DefaultAccounts>().Init(this.m_oTrail.MyInputData.MetaData.NumOfDefaultAccounts);
		} // CheckDefaultAccounts

		private void CheckExperianScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold;
			int nScore = this.m_oTrail.MyInputData.MetaData.ConsumerScore;

			if (nScore < nThreshold)
				StepFailed<ConsumerScore>().Init(nScore, nThreshold);
			else
				StepDone<ConsumerScore>().Init(nScore, nThreshold);
		} // CheckExperianScore

		private void CheckHmrcIsCompany() {
			bool isCompany = false;

			if (this.m_oTrail.MyInputData.HmrcBusinessNames.Count < 1) {
				StepDone<HmrcIsOfBusiness>().Init();
				return;
			} // if

			foreach (string hmrcName in this.m_oTrail.MyInputData.HmrcBusinessNames) {
				if (hmrcName.Equals(this.m_oTrail.MyInputData.CompanyName)) {
					isCompany = true;
					break;
				} // if
			} // for

			if (!isCompany) {
				StepFailed<HmrcIsOfBusiness>().Init(
					this.m_oTrail.MyInputData.HmrcBusinessNames,
					this.m_oTrail.MyInputData.CompanyName
				);
			} else {
				StepDone<HmrcIsOfBusiness>().Init(
					this.m_oTrail.MyInputData.HmrcBusinessNames,
					this.m_oTrail.MyInputData.CompanyName
				);
			} // if
		} // CheckHmrcIsCompany

		private void CheckTurnovers() {
			if (this.m_oTrail.MyInputData.IsTurnoverGood()) {
				StepDone<ThreeMonthsTurnover>().Init(
					this.m_oTrail.MyInputData.Turnover3M,
					this.m_oTrail.MyInputData.Turnover1Y,
					this.m_oTrail.MyInputData.Configuration.TurnoverDropQuarterRatio
				);
			} else {
				StepFailed<ThreeMonthsTurnover>().Init(
					this.m_oTrail.MyInputData.Turnover3M,
					this.m_oTrail.MyInputData.Turnover1Y,
					this.m_oTrail.MyInputData.Configuration.TurnoverDropQuarterRatio
				);
			} // if
		} // CheckTurnovers

		private void CheckInit() {
			int nAutoApprovedAmount = this.m_oTrail.RoundedAmount;

			if (nAutoApprovedAmount > 0)
				StepDone<InitialAssignment>().Init(this.m_oTrail.RoundedAmount);
			else
				StepFailed<InitialAssignment>().Init(this.m_oTrail.RoundedAmount);
		} // CheckInit

		private void CheckIsBroker() {
			if (this.isBrokerCustomer)
				StepFailed<IsBrokerCustomer>().Init();
			else
				StepDone<IsBrokerCustomer>().Init();
		} // CheckIsBroker

		private void CheckIsDirector() {
			if (!this.m_oTrail.MyInputData.MetaData.IsLimitedCompanyType) {
				StepDone<CustomerIsDirector>().Init(this.m_oTrail.MyInputData.MetaData.IsLimitedCompanyType);
				return;
			} // if

			bool isDirector = false;

			if (this.m_oTrail.MyInputData.DirectorNames.Count < 1) {
				StepFailed<CustomerIsDirector>().Init(this.m_oTrail.MyInputData.CustomerName.ToString());
				return;
			} // if

			foreach (Name directorName in this.m_oTrail.MyInputData.DirectorNames) {
				if (directorName.Equals(this.m_oTrail.MyInputData.CustomerName)) {
					isDirector = true;
					break;
				} // if
			} // for

			if (!isDirector) {
				var nc = new NameComparer(
					m_oTrail.MyInputData.CustomerName,
					m_oTrail.MyInputData.DirectorNames,
					this.db
				);

				foreach (Name name in m_oTrail.MyInputData.DirectorNames) {
					StringDifference firstNameDiff = nc[m_oTrail.MyInputData.CustomerName.FirstName, name.FirstName];

					if ((int)firstNameDiff >= (int)StringDifference.SoundVerySimilar) {
						StringDifference lastNameDiff = nc[m_oTrail.MyInputData.CustomerName.LastName, name.LastName];
						
						if ((int)lastNameDiff >= (int)StringDifference.SoundVerySimilar) {
							isDirector = true;
							break;
						} // if
					} // if
				} // for each name
			} // if

			if (!isDirector) {
				StepFailed<CustomerIsDirector>().Init(
					this.m_oTrail.MyInputData.CustomerName.ToString(),
					this.m_oTrail.MyInputData.DirectorNames.Select(x => x.ToString()).ToList()
				);
			} else {
				StepDone<CustomerIsDirector>().Init(
					this.m_oTrail.MyInputData.CustomerName.ToString(),
					this.m_oTrail.MyInputData.DirectorNames.Select(x => x.ToString()).ToList()
				);
			} // if
		} // CheckIsDirector

		private void CheckIsFraud() {
			bool fraudPassed =
				(this.m_oTrail.MyInputData.MetaData.PreviousManualApproveCount > 0) ||
				(this.m_oTrail.MyInputData.MetaData.FraudStatus == FraudStatus.Ok);

			if (fraudPassed)
				StepDone<FraudSuspect>().Init(this.m_oTrail.MyInputData.MetaData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(this.m_oTrail.MyInputData.MetaData.FraudStatus);
		} // CheckIsFraud

		private void CheckLateDays() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			if (this.m_oTrail.MyInputData.LatePayments.Count < 1) {
				StepDone<LatePayment>().Init(0, 0, Now, 0, Now, autoApproveMaxAllowedDaysLate);
				return;
			} // if

			foreach (Payment oPayment in this.m_oTrail.MyInputData.LatePayments) {
				StepFailed<LatePayment>().Init(
					oPayment.LoanID,
					oPayment.ScheduleID, oPayment.ScheduleDate,
					oPayment.TransactionID, oPayment.TransactionTime,
					autoApproveMaxAllowedDaysLate
				);
			} // for
		} // CheckLateDays

		private void CheckMedal() {
			if (this.medalClassification == EZBob.DatabaseLib.Model.Database.Medal.NoClassification)
				StepFailed<MedalIsGood>().Init((AutomationCalculator.Common.Medal)this.medalClassification);
			else
				StepDone<MedalIsGood>().Init((AutomationCalculator.Common.Medal)this.medalClassification);
		} // CheckMedal

		private void CheckOutstandingOffers(decimal outstandingOffers) {
			int autoApproveMaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers;

			if (outstandingOffers >= autoApproveMaxOutstandingOffers)
				StepFailed<OutstandingOffers>().Init(outstandingOffers, autoApproveMaxOutstandingOffers);
			else
				StepDone<OutstandingOffers>().Init(outstandingOffers, autoApproveMaxOutstandingOffers);
		} // CheckOutstandingOffers

		private void CheckRepaidRatio() {
			decimal autoApproveMinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion;

			if (this.m_oTrail.MyInputData.MetaData.RepaidRatio >= autoApproveMinRepaidPortion) {
				StepDone<OutstandingRepayRatio>().Init(
					this.m_oTrail.MyInputData.MetaData.RepaidRatio,
					autoApproveMinRepaidPortion
				);
			} else {
				StepFailed<OutstandingRepayRatio>().Init(
					this.m_oTrail.MyInputData.MetaData.RepaidRatio,
					autoApproveMinRepaidPortion
				);
			} // if
		} // CheckRepaidRatio

		private void CheckRollovers() {
			if (this.m_oTrail.MyInputData.MetaData.NumOfRollovers > 0)
				StepFailed<Rollovers>().Init();
			else
				StepDone<Rollovers>().Init();
		} // CheckRollovers

		private void CheckSeniority() {
			int autoApproveMinMpSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays;

			if (this.m_oTrail.MyInputData.MarketplaceSeniority < autoApproveMinMpSeniorityDays) {
				StepFailed<MarketplaceSeniority>().Init(
					this.m_oTrail.MyInputData.MarketplaceSeniority,
					autoApproveMinMpSeniorityDays
				);
			} else {
				StepDone<MarketplaceSeniority>().Init(
					this.m_oTrail.MyInputData.MarketplaceSeniority,
					autoApproveMinMpSeniorityDays
				);
			} // if
		} // CheckSeniority

		private void CheckTodaysApprovals() {
			int autoApproveMaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals;

			if (this.m_oTrail.MyInputData.MetaData.NumOfTodayAutoApproval >= autoApproveMaxDailyApprovals) {
				StepFailed<TodayApprovalCount>().Init(
					this.m_oTrail.MyInputData.MetaData.NumOfTodayAutoApproval,
					autoApproveMaxDailyApprovals
				);
			} else {
				StepDone<TodayApprovalCount>().Init(
					this.m_oTrail.MyInputData.MetaData.NumOfTodayAutoApproval,
					autoApproveMaxDailyApprovals
				);
			} // if
		} // CheckTodaysApprovals

		private void CheckTodaysLoans() {
			int autoApproveMaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans;

			if (this.m_oTrail.MyInputData.MetaData.TodayLoanSum >= autoApproveMaxTodayLoans)
				StepFailed<TodayLoans>().Init(this.m_oTrail.MyInputData.MetaData.TodayLoanSum, autoApproveMaxTodayLoans);
			else
				StepDone<TodayLoans>().Init(this.m_oTrail.MyInputData.MetaData.TodayLoanSum, autoApproveMaxTodayLoans);
		} // CheckTodaysLoans

		private void CheckTotalLoanCount() {
			StepDone<TotalLoanCount>().Init(this.m_oTrail.MyInputData.MetaData.TotalLoanCount);
		} // CheckTotalLoanCount

		private void CheckWorstCaisStatus(string allowedStatuses) {
			List<string> oAllowedStatuses = allowedStatuses.Split(',').ToList();

			List<string> diff = this.consumerCaisDetailWorstStatuses.Except(oAllowedStatuses)
				.ToList();

			if (diff.Count > 1)
				StepFailed<WorstCaisStatus>().Init(diff, this.consumerCaisDetailWorstStatuses, oAllowedStatuses);
			else
				StepDone<WorstCaisStatus>().Init(null, this.consumerCaisDetailWorstStatuses, oAllowedStatuses);
		} // CheckWorstCaisStatus

		private void ReduceOutstandingPrincipal() {
			this.m_oTrail.Amount = this.m_oTrail.SafeAmount - this.m_oTrail.MyInputData.MetaData.OutstandingPrincipal;

			if (this.m_oTrail.RoundedAmount < 0)
				this.m_oTrail.Amount = 0;

			if (this.m_oTrail.SafeAmount > 0.00000001m) {
				StepDone<ReduceOutstandingPrincipal>().Init(
					this.m_oTrail.MyInputData.MetaData.OutstandingPrincipal,
					this.m_oTrail.SafeAmount
				);
			} else {
				StepFailed<ReduceOutstandingPrincipal>().Init(
					this.m_oTrail.MyInputData.MetaData.OutstandingPrincipal,
					this.m_oTrail.SafeAmount
				);
			} // if
		} // ReduceOutstandingPrincipal

	} // class Approval
} // namespace
