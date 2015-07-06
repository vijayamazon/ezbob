namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using EZBob.DatabaseLib.Model.Database;

	public class Checker {
		public Checker(Agent oAgent) {
			this.agent = oAgent;
		} // constructor

		public virtual void Run() {
			InitOfficeHoursHandler();

			// Once a step is not passed there is no need to continue result-wise. However the
			// process continues because we want to pick all the possible reasons for not
			// approving a customer in order to compare different implementations of the process.

			Init();

			Medal();
			IsFraud();
			IsBrokerCustomer();
			CompanyIsDissolved();

			TodayApprovedCount();
			TodayOpenLoans();
			HourlyApprovals();
			LastHourApprovals();
			OutstandingOffers();

			Aml();
			CustomerStatus();
			CompanyScore();
			ConsumerScore();
			CustomerAge();
			Turnovers();
			CompanyAge();
			DefaultAccounts();
			IsDirector();
			HmrcIsRelevant();
			TotalLoanCount();
			CaisStatuses();
			Rollovers();
			LatePayments();
			CustomerOpenLoans();
			RepaidRatio();
			ReduceOutstandingPrincipal();
			RoundAmount();
			AllowedRange();
			Complete();
			AvailableFundsOverdraft();
		} // Run

		public T StepForceFailed<T>() where T : ATrace {
			ApprovedAmount = 0;
			return Trail.Negative<T>(false);
		} // StepForceFailed

		public virtual void CaisStatuses() {
			var diff = new List<string>(this.agent.FindBadCaisStatuses());

			if (diff.Count > 0)
				StepFailed<WorstCaisStatus>().Init(diff);
			else
				StepDone<WorstCaisStatus>().Init(null);
		} // CaisStatuses

		protected virtual T StepDone<T>() where T : ATrace {
			return Trail.Affirmative<T>(false);
		} // StepFailed

		protected virtual T StepFailed<T>() where T : ATrace {
			bool isStepEnabled = Trail.MyInputData.Configuration.IsTraceEnabled<T>();

			if (!isStepEnabled) {
				Trail.AddNote(
					"Step '" + typeof(T).FullName + "' has failed but is disabled hence marked as passed."
				);
			} // if

			return isStepEnabled
				? StepForceFailed<T>()
				: StepDone<T>();
		} // StepFailed

		private void RoundAmount() {
			decimal roundTo = Trail.MyInputData.Configuration.GetCashSliderStep;

			if (roundTo < 0.00000001m)
				roundTo = 1m;

			agent.Log.Debug(
				"Secondary before rounding: amount = {0}, minLoanAmount = {1}",
				Trail.SafeAmount,
				roundTo
			);

			Trail.Amount = roundTo * Math.Round(
				Trail.SafeAmount / roundTo, 0, MidpointRounding.AwayFromZero
			);

			agent.Log.Debug(
				"Secondary after rounding: amount = {0}, minLoanAmount = {1}",
				Trail.SafeAmount,
				roundTo
			);
		} // RoundAmount

		private ApprovalTrail Trail {
			get { return this.agent.Trail; }
		} // Trail

		private decimal ApprovedAmount {
			get { return this.agent.Trail.SafeAmount; }
			set { this.agent.Trail.Amount = value; }
		} // ApprovedAmount

		private decimal RoundedAmount {
			get { return this.agent.Trail.RoundedAmount; }
		} // RoundedAmount

		private DateTime Now {
			get { return this.agent.Now; }
		} // Now

		private void CompanyIsDissolved() {
			bool isDissolved =
				Trail.MyInputData.MetaData.CompanyDissolutionDate.HasValue &&
				Trail.MyInputData.MetaData.CompanyDissolutionDate.Value <= Now;

			if (isDissolved)
				StepFailed<CompanyIsDissolved>().Init(Trail.MyInputData.MetaData.CompanyDissolutionDate);
			else
				StepDone<CompanyIsDissolved>().Init(Trail.MyInputData.MetaData.CompanyDissolutionDate);
		} // CompanyIsDissolved

		private void IsDirector() {
			if (!Trail.MyInputData.MetaData.IsLimitedCompanyType) {
				StepDone<CustomerIsDirector>().Init(Trail.MyInputData.MetaData.IsLimitedCompanyType);
				return;
			} // if

			bool bIsDirector = false;

			if (Trail.MyInputData.DirectorNames.Count < 1) {
				StepFailed<CustomerIsDirector>().Init(Trail.MyInputData.CustomerName.ToString());
				return;
			} // if

			foreach (Name name in Trail.MyInputData.DirectorNames) {
				if (Trail.MyInputData.CustomerName.Equals(name)) {
					bIsDirector = true;
					break;
				} // if
			} // for each name

			if (!bIsDirector) {
				var nc = new NameComparer(
					Trail.MyInputData.CustomerName,
					Trail.MyInputData.DirectorNames,
					this.agent.DB
				);

				foreach (Name name in Trail.MyInputData.DirectorNames) {
					StringDifference firstNameDiff = nc[Trail.MyInputData.CustomerName.FirstName, name.FirstName];

					if ((int)firstNameDiff >= (int)StringDifference.SoundVerySimilar) {
						StringDifference lastNameDiff = nc[Trail.MyInputData.CustomerName.LastName, name.LastName];
						
						if ((int)lastNameDiff >= (int)StringDifference.SoundVerySimilar) {
							bIsDirector = true;
							break;
						} // if
					} // if
				} // for each name
			} // if

			if (bIsDirector) {
				StepDone<CustomerIsDirector>().Init(
					Trail.MyInputData.CustomerName.ToString(),
					Trail.MyInputData.DirectorNames.Select(n => n.ToString()).ToList()
				);
			} else {
				StepFailed<CustomerIsDirector>().Init(
					Trail.MyInputData.CustomerName.ToString(),
					Trail.MyInputData.DirectorNames.Select(n => n.ToString()).ToList()
				);
			} // if
		} // IsDirector

		private void HmrcIsRelevant() {
			bool bIsRelevant = false;

			if (Trail.MyInputData.HmrcBusinessNames.Count < 1) {
				StepDone<HmrcIsOfBusiness>()
					.Init();
				return;
			} // if

			foreach (string sName in Trail.MyInputData.HmrcBusinessNames) {
				if (Trail.MyInputData.CompanyName.Equals(sName)) {
					bIsRelevant = true;
					break;
				} // if
			} // for each name

			if (bIsRelevant)
				StepDone<HmrcIsOfBusiness>().Init(Trail.MyInputData.HmrcBusinessNames, Trail.MyInputData.CompanyName);
			else
				StepFailed<HmrcIsOfBusiness>().Init(Trail.MyInputData.HmrcBusinessNames, Trail.MyInputData.CompanyName);
		} // HmrcIsRelevant

		private void Medal() {
			if (Trail.MyInputData.Medal == AutomationCalculator.Common.Medal.NoClassification)
				StepFailed<MedalIsGood>().Init(Trail.MyInputData.Medal);
			else
				StepDone<MedalIsGood>().Init(Trail.MyInputData.Medal);
		} // Medal

		private void Init() {
			decimal approvedAmount = ApprovedAmount;

			if (approvedAmount > 0)
				StepDone<InitialAssignment>().Init(approvedAmount, Trail.MyInputData.MetaData.ValidationErrors);
			else
				StepFailed<InitialAssignment>().Init(approvedAmount, Trail.MyInputData.MetaData.ValidationErrors);
		} // Init

		private void IsFraud() {
			bool isFraudOk =
				(Trail.MyInputData.MetaData.PreviousManualApproveCount > 0) ||
				(Trail.MyInputData.MetaData.FraudStatus == FraudStatus.Ok);

			if (isFraudOk)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.MetaData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.MetaData.FraudStatus);
		} // IsFraud

		private void IsBrokerCustomer() {
			if (Trail.MyInputData.MetaData.IsBrokerCustomer)
				StepFailed<IsBrokerCustomer>().Init();
			else
				StepDone<IsBrokerCustomer>().Init();
		} // IsBrokerCustomer

		private void TodayApprovedCount() {
			int threshold = this.officeHoursHandler.IsWorkingTime
				? Trail.MyInputData.Configuration.MaxDailyApprovals
				: Trail.MyInputData.Configuration.OffHoursMaxDailyApprovals;

			int counter = this.officeHoursHandler.Current.ApprovalCount;

			if (counter >= threshold)
				StepFailed<TodayApprovalCount>().Init(counter, threshold);
			else
				StepDone<TodayApprovalCount>().Init(counter, threshold);
		} // TodayApprovedCount

		private void HourlyApprovals() {
			int threshold = this.officeHoursHandler.IsWorkingTime
				? Trail.MyInputData.Configuration.MaxHourlyApprovals
				: Trail.MyInputData.Configuration.OffHoursMaxHourlyApprovals;

			int counter = Trail.MyInputData.MetaData.NumOfHourlyAutoApprovals; 

			if (counter > threshold)
				StepFailed<HourlyApprovalCount>().Init(counter, threshold);
			else
				StepDone<HourlyApprovalCount>().Init(counter, threshold);
		} // HourlyApprovals

		private void LastHourApprovals() {
			int threshold = this.officeHoursHandler.IsWorkingTime
				? Trail.MyInputData.Configuration.MaxLastHourApprovals
				: Trail.MyInputData.Configuration.OffHoursMaxLastHourApprovals;

			int counter = Trail.MyInputData.MetaData.NumOfLastHourAutoApprovals;

			if (counter > threshold)
				StepFailed<LastHourApprovalCount>().Init(counter, threshold);
			else
				StepDone<LastHourApprovalCount>().Init(counter, threshold);
		} // LastHourApprovals

		private void TodayOpenLoans() {
			int threshold = this.officeHoursHandler.IsWorkingTime
				? Trail.MyInputData.Configuration.MaxTodayLoans
				: Trail.MyInputData.Configuration.OffHoursMaxTodayLoans;

			decimal amount = this.officeHoursHandler.Current.OpenLoanAmount;

			if (amount >= threshold)
				StepFailed<TodayLoans>().Init(amount, threshold);
			else
				StepDone<TodayLoans>().Init(amount, threshold);
		} // TodayOpenLoans

		private void OutstandingOffers() {
			int threshold = this.officeHoursHandler.IsWorkingTime
				? Trail.MyInputData.Configuration.MaxOutstandingOffers
				: Trail.MyInputData.Configuration.OffHoursMaxOutstandingOffers;

			decimal amount = Trail.MyInputData.ReservedFunds;

			if (amount >= threshold)
				StepFailed<OutstandingOffers>().Init(amount, threshold);
			else
				StepDone<OutstandingOffers>().Init(amount, threshold);
		} // OutstandingOffers

		private void Aml() {
			bool isAmlOk =
				(Trail.MyInputData.MetaData.PreviousManualApproveCount > 0) ||
				(0 == string.Compare(
					Trail.MyInputData.MetaData.AmlResult, "passed", StringComparison.InvariantCultureIgnoreCase)
				);

			if (isAmlOk)
				StepDone<AmlCheck>().Init(Trail.MyInputData.MetaData.AmlResult);
			else
				StepFailed<AmlCheck>().Init(Trail.MyInputData.MetaData.AmlResult);
		} // Aml

		private void CustomerStatus() {
			if (Trail.MyInputData.MetaData.CustomerStatusEnabled)
				StepDone<CustomerStatus>().Init(Trail.MyInputData.MetaData.CustomerStatusName);
			else
				StepFailed<CustomerStatus>().Init(Trail.MyInputData.MetaData.CustomerStatusName);
		} // CustomerStatus

		private void CompanyScore() {
			bool goodBusinessScore = 
				(Trail.MyInputData.MetaData.CompanyScore <= 0) ||
				(Trail.MyInputData.MetaData.CompanyScore >= Trail.MyInputData.Configuration.BusinessScoreThreshold);

			if (goodBusinessScore) {
				StepDone<BusinessScore>().Init(
					Trail.MyInputData.MetaData.CompanyScore,
					Trail.MyInputData.Configuration.BusinessScoreThreshold
				);
			} else {
				StepFailed<BusinessScore>().Init(
					Trail.MyInputData.MetaData.CompanyScore,
					Trail.MyInputData.Configuration.BusinessScoreThreshold
				);
			} // if
		} // CompayScore

		private void ConsumerScore() {
			if (Trail.MyInputData.MetaData.ConsumerScore >= Trail.MyInputData.Configuration.ExperianScoreThreshold) {
				StepDone<ConsumerScore>().Init(
					Trail.MyInputData.MetaData.ConsumerScore,
					Trail.MyInputData.Configuration.ExperianScoreThreshold
				);
			} else {
				StepFailed<ConsumerScore>().Init(
					Trail.MyInputData.MetaData.ConsumerScore,
					Trail.MyInputData.Configuration.ExperianScoreThreshold
				);
			} // if
		} // ConsumerScore

		private void CustomerAge() {
			// nAge: full number of years in customer's age.
			int nAge = Now.Year - Trail.MyInputData.MetaData.DateOfBirth.Year;

			if (Trail.MyInputData.MetaData.DateOfBirth.AddYears(nAge) > Now)
				nAge--; // this happens if customer had no birthday this year.

			bool goodAge = 
				(Trail.MyInputData.Configuration.CustomerMinAge <= nAge) &&
				(nAge <= Trail.MyInputData.Configuration.CustomerMaxAge);

			if (goodAge) {
				StepDone<Age>().Init(
					nAge,
					Trail.MyInputData.Configuration.CustomerMinAge,
					Trail.MyInputData.Configuration.CustomerMaxAge
				);
			} else {
				StepFailed<Age>().Init(
					nAge,
					Trail.MyInputData.Configuration.CustomerMinAge,
					Trail.MyInputData.Configuration.CustomerMaxAge
				);
			} // if
		} // CustomerAge

		private void Turnovers() {
			if (Trail.MyInputData.IsTurnoverGood()) {
				StepDone<ThreeMonthsTurnover>().Init(
					Trail.MyInputData.Turnover3M,
					Trail.MyInputData.Turnover1Y,
					Trail.MyInputData.Configuration.TurnoverDropQuarterRatio
				);
			} else {
				StepFailed<ThreeMonthsTurnover>().Init(
					Trail.MyInputData.Turnover3M,
					Trail.MyInputData.Turnover1Y,
					Trail.MyInputData.Configuration.TurnoverDropQuarterRatio
				);
			} // if
		} // Turnovers

		private void CompanyAge() {
			if (Trail.MyInputData.MarketplaceSeniority >= Trail.MyInputData.Configuration.MinMPSeniorityDays) {
				StepDone<MarketplaceSeniority>().Init(
					Trail.MyInputData.MarketplaceSeniority,
					Trail.MyInputData.Configuration.MinMPSeniorityDays
				);
			} else {
				StepFailed<MarketplaceSeniority>().Init(
					Trail.MyInputData.MarketplaceSeniority,
					Trail.MyInputData.Configuration.MinMPSeniorityDays
				);
			}
		} // CompanyAge

		private void DefaultAccounts() {
			if (Trail.MyInputData.MetaData.NumOfDefaultAccounts > 0)
				StepFailed<DefaultAccounts>().Init(Trail.MyInputData.MetaData.NumOfDefaultAccounts);
			else
				StepDone<DefaultAccounts>().Init(Trail.MyInputData.MetaData.NumOfDefaultAccounts);
		} // DefaultAccounts

		private void TotalLoanCount() {
			StepDone<TotalLoanCount>().Init(Trail.MyInputData.MetaData.TotalLoanCount);
		} // TotalLoanCount

		private void Rollovers() {
			if (Trail.MyInputData.MetaData.NumOfRollovers > 0)
				StepFailed<Rollovers>().Init();
			else
				StepDone<Rollovers>().Init();
		} // Rollovers

		private void LatePayments() {
			bool bHasLatePayments = false;

			foreach (Payment lp in Trail.MyInputData.LatePayments) {
				if (lp.IsLate(Trail.MyInputData.Configuration.MaxAllowedDaysLate)) {
					bHasLatePayments = true;

					lp.Fill(StepFailed<LatePayment>(), Trail.MyInputData.Configuration.MaxAllowedDaysLate);
				} // if
			} // for each

			if (!bHasLatePayments)
				StepDone<LatePayment>().Init(0, 0, Now, 0, Now, Trail.MyInputData.Configuration.MaxAllowedDaysLate);
		} // LatePayments

		private void CustomerOpenLoans() {
			if (Trail.MyInputData.MetaData.OpenLoanCount > Trail.MyInputData.Configuration.MaxNumOfOutstandingLoans) {
				StepFailed<OutstandingLoanCount>().Init(
					Trail.MyInputData.MetaData.OpenLoanCount,
					Trail.MyInputData.Configuration.MaxNumOfOutstandingLoans
				);
			} else {
				StepDone<OutstandingLoanCount>().Init(
					Trail.MyInputData.MetaData.OpenLoanCount,
					Trail.MyInputData.Configuration.MaxNumOfOutstandingLoans
				);
			} // if
		} // CustomerOpenLoans

		private void RepaidRatio() {
			if (Trail.MyInputData.MetaData.RepaidRatio >= Trail.MyInputData.Configuration.MinRepaidPortion) {
				StepDone<OutstandingRepayRatio>().Init(
					Trail.MyInputData.MetaData.RepaidRatio,
					Trail.MyInputData.Configuration.MinRepaidPortion
				);
			} else {
				StepFailed<OutstandingRepayRatio>().Init(
					Trail.MyInputData.MetaData.RepaidRatio,
					Trail.MyInputData.Configuration.MinRepaidPortion
				);
			} // if
		} // RepaidRatio

		private void ReduceOutstandingPrincipal() {
			ApprovedAmount -= Trail.MyInputData.MetaData.OutstandingPrincipal;

			if (ApprovedAmount < 0)
				ApprovedAmount = 0;

			if (ApprovedAmount > 0.00000001m) {
				StepDone<ReduceOutstandingPrincipal>().Init(
					Trail.MyInputData.MetaData.OutstandingPrincipal,
					ApprovedAmount
				);
			} else {
				StepFailed<ReduceOutstandingPrincipal>().Init(
					Trail.MyInputData.MetaData.OutstandingPrincipal,
					ApprovedAmount
				);
			} // if
		} // ReduceOutstandingPrincipal

		private void AllowedRange() {
			decimal nApprovedAmount = ApprovedAmount;

			if (Trail.MyInputData.Configuration.IsSilent) {
				StepDone<AmountOutOfRangle>().Init(
					nApprovedAmount,
					Trail.MyInputData.Configuration.MinLoan,
					Trail.MyInputData.Configuration.MaxAmount
				);

				return;
			} // if

			bool inRange =
				(Trail.MyInputData.Configuration.MinLoan <= nApprovedAmount) &&
				(nApprovedAmount <= Trail.MyInputData.Configuration.MaxAmount);

			if (inRange) {
				StepDone<AmountOutOfRangle>().Init(
					nApprovedAmount,
					Trail.MyInputData.Configuration.MinLoan,
					Trail.MyInputData.Configuration.MaxAmount
				);
			} else {
				StepFailed<AmountOutOfRangle>().Init(
					nApprovedAmount,
					Trail.MyInputData.Configuration.MinLoan,
					Trail.MyInputData.Configuration.MaxAmount
				);
			} // if
		} // AllowedRange

		private void Complete() {
			decimal nApprovedAmount = ApprovedAmount;

			if (nApprovedAmount > 0)
				StepDone<Complete>().Init(nApprovedAmount);
			else
				StepFailed<Complete>().Init(nApprovedAmount);
		} // Complete

		private void AvailableFundsOverdraft() {
			decimal autoApprovedAmount = RoundedAmount;
			decimal availableFunds = Trail.MyInputData.AvailableFunds;
			decimal allowedOverdraft = -Math.Abs(Trail.MyInputData.Configuration.AvailableFundsOverdraft);

			decimal reminder = availableFunds - autoApprovedAmount;

			if (reminder >= allowedOverdraft)
				StepDone<AvailableFundsOverdraft>().Init(reminder, allowedOverdraft);
			else
				StepFailed<AvailableFundsOverdraft>().Init(reminder, allowedOverdraft);
		} // AvailableFundsOverdraft

		private void InitOfficeHoursHandler() {
			this.officeHoursHandler = new OfficeHoursHandler(
				this.agent.Now,
				this.agent.Trail.MyInputData.Configuration.OfficeTimeStart,
				this.agent.Trail.MyInputData.Configuration.OfficeTimeEnd,
				this.agent.Trail.MyInputData.Configuration.Weekend
			);

			this.officeHoursHandler.ApprovalCount[0] = this.agent.Trail.MyInputData.MetaData.NumOfTodayAutoApproval;
			this.officeHoursHandler.ApprovalCount[1] = this.agent.Trail.MyInputData.MetaData.NumOfYesterdayAutoApproval;

			this.officeHoursHandler.OpenLoanAmount[0] = this.agent.Trail.MyInputData.MetaData.TodayLoanSum;
			this.officeHoursHandler.OpenLoanAmount[1] = this.agent.Trail.MyInputData.MetaData.YesterdayLoanSum;
		} // InitOfficeHoursHandler

		private readonly Agent agent;
		private OfficeHoursHandler officeHoursHandler;
	} // class Checker
} // namespace
