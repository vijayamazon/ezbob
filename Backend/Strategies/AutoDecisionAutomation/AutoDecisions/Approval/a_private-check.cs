namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval {
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
				CheckWorstCaisStatus(this.trail.MyInputData.MetaData.TotalLoanCount > 0
					? CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithLoan
					: CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithoutLoan
				);
				CheckRollovers();
				CheckLateDays();
				CheckCustomerOpenLoans();
				CheckRepaidRatio();
				ReduceOutstandingPrincipal();
				RoundAmount();
				CheckAllowedRange();
				CheckComplete();
			} catch (Exception ex) {
				StepForceFailed<ExceptionThrown>().Init(ex);
			} // try

			this.log.Debug(
				"Primary: checking if auto approval should take place for customer {0} complete.",
				this.customerId
			);

			this.log.Msg("Primary: auto approved amount: {0}. {1}", this.trail.RoundedAmount, this.trail);
		} // CheckAutoApprovalConformance

		private void RoundAmount() {
			decimal roundTo = CurrentValues.Instance.GetCashSliderStep;

			if (roundTo < 0.00000001m)
				roundTo = 1m;

			this.log.Debug(
				"Primary before rounding: amount = {0}, minLoanAmount = {1}",
				this.trail.SafeAmount,
				roundTo
			);

			this.trail.Amount = roundTo * Math.Round(
				this.trail.SafeAmount / roundTo, 0, MidpointRounding.AwayFromZero
			);

			this.log.Debug(
				"Primary after rounding: amount = {0}, minLoanAmount = {1}",
				this.trail.SafeAmount,
				roundTo
			);
		} // RoundAmount

		private void CheckCompanyIsDissolved() {
			bool isDissolved =
				this.trail.MyInputData.MetaData.CompanyDissolutionDate.HasValue &&
				this.trail.MyInputData.MetaData.CompanyDissolutionDate.Value <= Now;

			if (isDissolved)
				StepFailed<CompanyIsDissolved>().Init(this.trail.MyInputData.MetaData.CompanyDissolutionDate);
			else
				StepDone<CompanyIsDissolved>().Init(this.trail.MyInputData.MetaData.CompanyDissolutionDate);
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
			if (this.trail.MyInputData.Configuration.IsSilent) {
				StepDone<AmountOutOfRangle>()
					.Init(this.trail.RoundedAmount, this.trail.MyInputData.Configuration.IsSilent);
			} else {
				int autoApproveMinAmount = this.trail.MyInputData.Configuration.MinLoan;
				int autoApproveMaxAmount = this.trail.MyInputData.Configuration.MaxAmount;

				if (this.trail.RoundedAmount < autoApproveMinAmount || this.trail.RoundedAmount > autoApproveMaxAmount) {
					StepFailed<AmountOutOfRangle>().Init(
						this.trail.RoundedAmount,
						autoApproveMinAmount,
						autoApproveMaxAmount
					);
				} else {
					StepDone<AmountOutOfRangle>().Init(
						this.trail.RoundedAmount,
						autoApproveMinAmount,
						autoApproveMaxAmount
					);
				}
			} // if
		} // CheckAllowedRange

		private void CheckAMLResult() {
			bool amlPassed =
				(this.trail.MyInputData.MetaData.PreviousManualApproveCount > 0) ||
				(this.trail.MyInputData.MetaData.AmlResult == "Passed");

			if (amlPassed)
				StepDone<AmlCheck>().Init(this.trail.MyInputData.MetaData.AmlResult);
			else
				StepFailed<AmlCheck>().Init(this.trail.MyInputData.MetaData.AmlResult);
		} // CheckAMLResult

		private void CheckBusinessScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold;
			int nScore = this.trail.MyInputData.MetaData.CompanyScore;

			if (nScore <= 0)
				StepDone<BusinessScore>().Init(nScore, nThreshold);
			else if (nScore < nThreshold)
				StepFailed<BusinessScore>().Init(nScore, nThreshold);
			else
				StepDone<BusinessScore>().Init(nScore, nThreshold);
		} // CheckBusinessScore

		private void CheckComplete() {
			int nAutoApprovedAmount = this.trail.RoundedAmount;

			if (nAutoApprovedAmount > 0)
				StepDone<Complete>().Init(nAutoApprovedAmount);
			else
				StepFailed<Complete>().Init(nAutoApprovedAmount);
		} // CheckComplete

		private void CheckCustomerOpenLoans() {
			int autoApproveMaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans;

			if (this.trail.MyInputData.MetaData.OpenLoanCount > autoApproveMaxNumOfOutstandingLoans) {
				StepFailed<OutstandingLoanCount>().Init(
					this.trail.MyInputData.MetaData.OpenLoanCount,
					autoApproveMaxNumOfOutstandingLoans
				);
			} else {
				StepDone<OutstandingLoanCount>().Init(
					this.trail.MyInputData.MetaData.OpenLoanCount,
					autoApproveMaxNumOfOutstandingLoans
				);
			} // if
		} // CheckCustomerOpenLoans

		private void CheckCustomerStatus() {
			if (!this.trail.MyInputData.MetaData.CustomerStatusEnabled)
				StepFailed<CustomerStatus>().Init(this.trail.MyInputData.MetaData.CustomerStatusName);
			else
				StepDone<CustomerStatus>().Init(this.trail.MyInputData.MetaData.CustomerStatusName);
		} // CheckCustomerStatus

		private void CheckDefaultAccounts() {
			if (this.trail.MyInputData.MetaData.NumOfDefaultAccounts > 0)
				StepFailed<DefaultAccounts>().Init(this.trail.MyInputData.MetaData.NumOfDefaultAccounts);
			else
				StepDone<DefaultAccounts>().Init(this.trail.MyInputData.MetaData.NumOfDefaultAccounts);
		} // CheckDefaultAccounts

		private void CheckExperianScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold;
			int nScore = this.trail.MyInputData.MetaData.ConsumerScore;

			if (nScore < nThreshold)
				StepFailed<ConsumerScore>().Init(nScore, nThreshold);
			else
				StepDone<ConsumerScore>().Init(nScore, nThreshold);
		} // CheckExperianScore

		private void CheckHmrcIsCompany() {
			bool isCompany = false;

			if (this.trail.MyInputData.HmrcBusinessNames.Count < 1) {
				StepDone<HmrcIsOfBusiness>().Init();
				return;
			} // if

			foreach (string hmrcName in this.trail.MyInputData.HmrcBusinessNames) {
				if (hmrcName.Equals(this.trail.MyInputData.CompanyName)) {
					isCompany = true;
					break;
				} // if
			} // for

			if (!isCompany) {
				StepFailed<HmrcIsOfBusiness>().Init(
					this.trail.MyInputData.HmrcBusinessNames,
					this.trail.MyInputData.CompanyName
				);
			} else {
				StepDone<HmrcIsOfBusiness>().Init(
					this.trail.MyInputData.HmrcBusinessNames,
					this.trail.MyInputData.CompanyName
				);
			} // if
		} // CheckHmrcIsCompany

		private void CheckTurnovers() {
			if (this.trail.MyInputData.IsTurnoverGood()) {
				StepDone<ThreeMonthsTurnover>().Init(
					this.trail.MyInputData.Turnover3M,
					this.trail.MyInputData.Turnover1Y,
					this.trail.MyInputData.Configuration.TurnoverDropQuarterRatio
				);
			} else {
				StepFailed<ThreeMonthsTurnover>().Init(
					this.trail.MyInputData.Turnover3M,
					this.trail.MyInputData.Turnover1Y,
					this.trail.MyInputData.Configuration.TurnoverDropQuarterRatio
				);
			} // if
		} // CheckTurnovers

		private void CheckInit() {
			int nAutoApprovedAmount = this.trail.RoundedAmount;

			if (nAutoApprovedAmount > 0)
				StepDone<InitialAssignment>().Init(this.trail.RoundedAmount);
			else
				StepFailed<InitialAssignment>().Init(this.trail.RoundedAmount);
		} // CheckInit

		private void CheckIsBroker() {
			if (this.isBrokerCustomer)
				StepFailed<IsBrokerCustomer>().Init();
			else
				StepDone<IsBrokerCustomer>().Init();
		} // CheckIsBroker

		private void CheckIsDirector() {
			if (!this.trail.MyInputData.MetaData.IsLimitedCompanyType) {
				StepDone<CustomerIsDirector>().Init(this.trail.MyInputData.MetaData.IsLimitedCompanyType);
				return;
			} // if

			bool isDirector = false;

			if (this.trail.MyInputData.DirectorNames.Count < 1) {
				StepFailed<CustomerIsDirector>().Init(this.trail.MyInputData.CustomerName.ToString());
				return;
			} // if

			foreach (Name directorName in this.trail.MyInputData.DirectorNames) {
				if (directorName.Equals(this.trail.MyInputData.CustomerName)) {
					isDirector = true;
					break;
				} // if
			} // for

			if (!isDirector) {
				var nc = new NameComparer(
					this.trail.MyInputData.CustomerName,
					this.trail.MyInputData.DirectorNames,
					this.db
				);

				foreach (Name name in this.trail.MyInputData.DirectorNames) {
					StringDifference firstNameDiff = nc[this.trail.MyInputData.CustomerName.FirstName, name.FirstName];

					if ((int)firstNameDiff >= (int)StringDifference.SoundVerySimilar) {
						StringDifference lastNameDiff = nc[this.trail.MyInputData.CustomerName.LastName, name.LastName];
						
						if ((int)lastNameDiff >= (int)StringDifference.SoundVerySimilar) {
							isDirector = true;
							break;
						} // if
					} // if
				} // for each name
			} // if

			if (!isDirector) {
				StepFailed<CustomerIsDirector>().Init(
					this.trail.MyInputData.CustomerName.ToString(),
					this.trail.MyInputData.DirectorNames.Select(x => x.ToString()).ToList()
				);
			} else {
				StepDone<CustomerIsDirector>().Init(
					this.trail.MyInputData.CustomerName.ToString(),
					this.trail.MyInputData.DirectorNames.Select(x => x.ToString()).ToList()
				);
			} // if
		} // CheckIsDirector

		private void CheckIsFraud() {
			bool fraudPassed =
				(this.trail.MyInputData.MetaData.PreviousManualApproveCount > 0) ||
				(this.trail.MyInputData.MetaData.FraudStatus == FraudStatus.Ok);

			if (fraudPassed)
				StepDone<FraudSuspect>().Init(this.trail.MyInputData.MetaData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(this.trail.MyInputData.MetaData.FraudStatus);
		} // CheckIsFraud

		private void CheckLateDays() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			if (this.trail.MyInputData.LatePayments.Count < 1) {
				StepDone<LatePayment>().Init(0, 0, Now, 0, Now, autoApproveMaxAllowedDaysLate);
				return;
			} // if

			foreach (Payment oPayment in this.trail.MyInputData.LatePayments) {
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

			if (this.trail.MyInputData.MetaData.RepaidRatio >= autoApproveMinRepaidPortion) {
				StepDone<OutstandingRepayRatio>().Init(
					this.trail.MyInputData.MetaData.RepaidRatio,
					autoApproveMinRepaidPortion
				);
			} else {
				StepFailed<OutstandingRepayRatio>().Init(
					this.trail.MyInputData.MetaData.RepaidRatio,
					autoApproveMinRepaidPortion
				);
			} // if
		} // CheckRepaidRatio

		private void CheckRollovers() {
			if (this.trail.MyInputData.MetaData.NumOfRollovers > 0)
				StepFailed<Rollovers>().Init();
			else
				StepDone<Rollovers>().Init();
		} // CheckRollovers

		private void CheckSeniority() {
			int autoApproveMinMpSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays;

			if (this.trail.MyInputData.MarketplaceSeniority < autoApproveMinMpSeniorityDays) {
				StepFailed<MarketplaceSeniority>().Init(
					this.trail.MyInputData.MarketplaceSeniority,
					autoApproveMinMpSeniorityDays
				);
			} else {
				StepDone<MarketplaceSeniority>().Init(
					this.trail.MyInputData.MarketplaceSeniority,
					autoApproveMinMpSeniorityDays
				);
			} // if
		} // CheckSeniority

		private void CheckTodaysApprovals() {
			int autoApproveMaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals;

			if (this.trail.MyInputData.MetaData.NumOfTodayAutoApproval >= autoApproveMaxDailyApprovals) {
				StepFailed<TodayApprovalCount>().Init(
					this.trail.MyInputData.MetaData.NumOfTodayAutoApproval,
					autoApproveMaxDailyApprovals
				);
			} else {
				StepDone<TodayApprovalCount>().Init(
					this.trail.MyInputData.MetaData.NumOfTodayAutoApproval,
					autoApproveMaxDailyApprovals
				);
			} // if
		} // CheckTodaysApprovals

		private void CheckTodaysLoans() {
			int autoApproveMaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans;

			if (this.trail.MyInputData.MetaData.TodayLoanSum >= autoApproveMaxTodayLoans)
				StepFailed<TodayLoans>().Init(this.trail.MyInputData.MetaData.TodayLoanSum, autoApproveMaxTodayLoans);
			else
				StepDone<TodayLoans>().Init(this.trail.MyInputData.MetaData.TodayLoanSum, autoApproveMaxTodayLoans);
		} // CheckTodaysLoans

		private void CheckTotalLoanCount() {
			StepDone<TotalLoanCount>().Init(this.trail.MyInputData.MetaData.TotalLoanCount);
		} // CheckTotalLoanCount

		private void CheckWorstCaisStatus(string allowedStatuses) {
			var diff = new List<string>(FindBadCaisStatuses());

			if (diff.Count > 0)
				StepFailed<WorstCaisStatus>().Init(diff);
			else
				StepDone<WorstCaisStatus>().Init(null);
		} // CheckWorstCaisStatus

		private void ReduceOutstandingPrincipal() {
			this.trail.Amount = this.trail.SafeAmount - this.trail.MyInputData.MetaData.OutstandingPrincipal;

			if (this.trail.RoundedAmount < 0)
				this.trail.Amount = 0;

			if (this.trail.SafeAmount > 0.00000001m) {
				StepDone<ReduceOutstandingPrincipal>().Init(
					this.trail.MyInputData.MetaData.OutstandingPrincipal,
					this.trail.SafeAmount
				);
			} else {
				StepFailed<ReduceOutstandingPrincipal>().Init(
					this.trail.MyInputData.MetaData.OutstandingPrincipal,
					this.trail.SafeAmount
				);
			} // if
		} // ReduceOutstandingPrincipal

	} // class Approval
} // namespace
