namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using EZBob.DatabaseLib.Model.Database;

	internal class Checker {
		#region public

		#region constructor

		public Checker(Agent oAgent) {
			m_oAgent = oAgent;
		} // constructor

		#endregion constructor

		#region method Run

		public void Run() {
			// Once a step is not passed there is no need to continue result-wise. However the
			// process continues because we want to pick all the possible reasons for not
			// approving a customer in order to compare different implementations of the process.

			Init();

			Medal();
			IsFraud();
			IsBrokerCustomer();
			TodayApprovedCount();
			TodayOpenLoans();
			OutstandingOffers();
			Aml();
			CustomerStatus();
			CompanyScore();
			ConsumerScore();
			CustomerAge();
			OnlineTurnovers();
			HmrcTurnovers();
			CompanyAge();
			DefaultAccounts();
			IsDirector();
			HmrcIsRelevant();
			TotalLoanCount();
			CaisStatuses(Trail.MyInputData.MetaData.TotalLoanCount > 0
				? Trail.MyInputData.Configuration.GetAllowedCaisStatusesWithLoan()
				: Trail.MyInputData.Configuration.GetAllowedCaisStatusesWithoutLoan()
			);
			Rollovers();
			LatePayments();
			CustomerOpenLoans();
			RepaidRatio();
			ReduceOutstandingPrincipal();
			AllowedRange();
			Complete();
		} // Run

		#endregion method Run

		#region method StepFailed

		public T StepFailed<T>() where T : ATrace {
			ApprovedAmount = 0;
			return Trail.Negative<T>(false);
		} // StepFailed

		#endregion method StepFailed

		#endregion public

		#region private

		#region steps

		#region method IsDirector

		private void IsDirector() {
			if (!Trail.MyInputData.MetaData.IsLimitedCompanyType) {
				StepDone<CustomerIsDirector>().Init(Trail.MyInputData.MetaData.IsLimitedCompanyType);
				return;
			}

			bool bIsDirector = false;

			if (Trail.MyInputData.DirectorNames.Count < 1) {
				StepFailed<CustomerIsDirector>().Init(
					Trail.MyInputData.CustomerName.ToString() 
				);
				return;
			} // if

			foreach (Name name in Trail.MyInputData.DirectorNames) {
				if (Trail.MyInputData.CustomerName.Equals(name)) {
					bIsDirector = true;
					break;
				} // if
			} // for each name

			if (bIsDirector) {
				StepDone<CustomerIsDirector>().Init(
					Trail.MyInputData.CustomerName.ToString(),
					Trail.MyInputData.DirectorNames.Select(n => n.ToString()).ToList()
				);
			}
			else {
				StepFailed<CustomerIsDirector>().Init(
					Trail.MyInputData.CustomerName.ToString(),
					Trail.MyInputData.DirectorNames.Select(n => n.ToString()).ToList()
				);
			}
		} // IsDirector

		#endregion method IsDirector

		#region method HmrcIsRelevant

		private void HmrcIsRelevant() {
			bool bIsRelevant = false;

			if (Trail.MyInputData.HmrcBusinessNames.Count < 1) {
				StepDone<HmrcIsOfBusiness>().Init();
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

		#endregion method HmrcIsRelevant

		#region method Medal

		private void Medal() {
			if (Trail.MyInputData.Medal == AutomationCalculator.Common.Medal.NoClassification)
				StepFailed<MedalIsGood>().Init(Trail.MyInputData.Medal);
			else
				StepDone<MedalIsGood>().Init(Trail.MyInputData.Medal);
		} // Medal

		#endregion method Medal

		#region method Init

		private void Init() {
			decimal approvedAmount = ApprovedAmount;

			if ((approvedAmount > 0) && (Trail.MyInputData.MetaData.ValidationErrors.Count == 0))
				StepDone<InitialAssignment>().Init(approvedAmount, Trail.MyInputData.MetaData.ValidationErrors);
			else
				StepFailed<InitialAssignment>().Init(approvedAmount, Trail.MyInputData.MetaData.ValidationErrors);
		} // Init

		#endregion method Init

		#region method IsFraud

		private void IsFraud() {
			if (Trail.MyInputData.MetaData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.MetaData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.MetaData.FraudStatus);
		} // IsFraud

		#endregion method IsFraud

		#region method IsBrokerCustomer

		private void IsBrokerCustomer() {
			if (Trail.MyInputData.MetaData.IsBrokerCustomer)
				StepFailed<IsBrokerCustomer>().Init();
			else
				StepDone<IsBrokerCustomer>().Init();
		} // IsBrokerCustomer

		#endregion method IsBrokerCustomer

		#region method TodayApprovedCount

		private void TodayApprovedCount() {
			if (Trail.MyInputData.MetaData.NumOfTodayAutoApproval > Trail.MyInputData.Configuration.MaxDailyApprovals)
				StepFailed<TodayApprovalCount>().Init(Trail.MyInputData.MetaData.NumOfTodayAutoApproval, Trail.MyInputData.Configuration.MaxDailyApprovals);
			else
				StepDone<TodayApprovalCount>().Init(Trail.MyInputData.MetaData.NumOfTodayAutoApproval, Trail.MyInputData.Configuration.MaxDailyApprovals);
		} // TodayApprovedCount

		#endregion method TodayApprovedCount

		#region method TodayOpenLoans

		private void TodayOpenLoans() {
			if (Trail.MyInputData.MetaData.TodayLoanSum > Trail.MyInputData.Configuration.MaxTodayLoans)
				StepFailed<TodayLoans>().Init(Trail.MyInputData.MetaData.TodayLoanSum, Trail.MyInputData.Configuration.MaxTodayLoans);
			else
				StepDone<TodayLoans>().Init(Trail.MyInputData.MetaData.TodayLoanSum, Trail.MyInputData.Configuration.MaxTodayLoans);
		} // TodayOpenLoans

		#endregion method TodayOpenLoans

		#region method OutstandingOffers

		private void OutstandingOffers() {
			if (Trail.MyInputData.ReservedFunds > Trail.MyInputData.Configuration.MaxOutstandingOffers)
				StepFailed<OutstandingOffers>().Init(Trail.MyInputData.ReservedFunds, Trail.MyInputData.Configuration.MaxOutstandingOffers);
			else
				StepDone<OutstandingOffers>().Init(Trail.MyInputData.ReservedFunds, Trail.MyInputData.Configuration.MaxOutstandingOffers);
		} // OutstandingOffers

		#endregion method OutstandingOffers

		#region method Aml

		private void Aml() {
			if (0 == string.Compare(Trail.MyInputData.MetaData.AmlResult, "passed", StringComparison.InvariantCultureIgnoreCase))
				StepDone<AmlCheck>().Init(Trail.MyInputData.MetaData.AmlResult);
			else
				StepFailed<AmlCheck>().Init(Trail.MyInputData.MetaData.AmlResult);
		} // Aml

		#endregion method Aml

		#region method CustomerStatus

		private void CustomerStatus() {
			if (Trail.MyInputData.MetaData.CustomerStatusEnabled)
				StepDone<CustomerStatus>().Init(Trail.MyInputData.MetaData.CustomerStatusName);
			else
				StepFailed<CustomerStatus>().Init(Trail.MyInputData.MetaData.CustomerStatusName);
		} // CustomerStatus

		#endregion method CustomerStatus

		#region method CompanyScore

		private void CompanyScore() {
			if ((Trail.MyInputData.MetaData.CompanyScore <= 0) || (Trail.MyInputData.MetaData.CompanyScore >= Trail.MyInputData.Configuration.BusinessScoreThreshold))
				StepDone<BusinessScore>().Init(Trail.MyInputData.MetaData.CompanyScore, Trail.MyInputData.Configuration.BusinessScoreThreshold);
			else
				StepFailed<BusinessScore>().Init(Trail.MyInputData.MetaData.CompanyScore, Trail.MyInputData.Configuration.BusinessScoreThreshold);
		} // CompayScore

		#endregion method CompanyScore

		#region method ConsumerScore

		private void ConsumerScore() {
			if (Trail.MyInputData.MetaData.ConsumerScore >= Trail.MyInputData.Configuration.ExperianScoreThreshold)
				StepDone<ConsumerScore>().Init(Trail.MyInputData.MetaData.ConsumerScore, Trail.MyInputData.Configuration.ExperianScoreThreshold);
			else
				StepFailed<ConsumerScore>().Init(Trail.MyInputData.MetaData.ConsumerScore, Trail.MyInputData.Configuration.ExperianScoreThreshold);
		} // ConsumerScore

		#endregion method ConsumerScore

		#region method CustomerAge

		private void CustomerAge() {
			// nAge: full number of years in customer's age.
			int nAge = Now.Year - Trail.MyInputData.MetaData.DateOfBirth.Year;

			if (Trail.MyInputData.MetaData.DateOfBirth.AddYears(nAge) > Now) // this happens if customer had no birthday this year.
				nAge--;

			if ((Trail.MyInputData.Configuration.CustomerMinAge <= nAge) && (nAge <= Trail.MyInputData.Configuration.CustomerMaxAge))
				StepDone<Age>().Init(nAge, Trail.MyInputData.Configuration.CustomerMinAge, Trail.MyInputData.Configuration.CustomerMaxAge);
			else
				StepFailed<Age>().Init(nAge, Trail.MyInputData.Configuration.CustomerMinAge, Trail.MyInputData.Configuration.CustomerMaxAge);
		} // CustomerAge

		#endregion method CustomerAge

		#region method OnlineTurnovers

		private void OnlineTurnovers() {
			if (!Trail.MyInputData.HasOnline && Trail.MyInputData.HasHmrc) {
				StepDone<OnlineTurnoverAge>().Init(Trail.MyInputData.HasOnline);
				return;
			} // if

			if (Trail.MyInputData.IsOnlineTurnoverTooOld())
				StepFailed<OnlineTurnoverAge>().Init(Trail.MyInputData.OnlineUpdateTime, Trail.MyInputData.DataAsOf);
			else
				StepDone<OnlineTurnoverAge>().Init(Trail.MyInputData.OnlineUpdateTime, Trail.MyInputData.DataAsOf);

			if (Trail.MyInputData.IsOnlineTurnoverGood(1))
				StepDone<OnlineOneMonthTurnover>().Init(Trail.MyInputData.OnlineTurnover1M, Trail.MyInputData.OnlineTurnover1Y, Trail.MyInputData.Configuration.OnlineTurnoverDropMonthRatio);
			else
				StepFailed<OnlineOneMonthTurnover>().Init(Trail.MyInputData.OnlineTurnover1M, Trail.MyInputData.OnlineTurnover1Y, Trail.MyInputData.Configuration.OnlineTurnoverDropMonthRatio);

			if (Trail.MyInputData.IsOnlineTurnoverGood(3))
				StepDone<OnlineThreeMonthsTurnover>().Init(Trail.MyInputData.OnlineTurnover3M, Trail.MyInputData.OnlineTurnover1Y, Trail.MyInputData.Configuration.OnlineTurnoverDropQuarterRatio);
			else
				StepFailed<OnlineThreeMonthsTurnover>().Init(Trail.MyInputData.OnlineTurnover3M, Trail.MyInputData.OnlineTurnover1Y, Trail.MyInputData.Configuration.OnlineTurnoverDropQuarterRatio);
		} // OnlineTurnovers

		#endregion method OnlineTurnovers

		#region method HmrcTurnovers

		private void HmrcTurnovers() {
			if (!Trail.MyInputData.HasHmrc && Trail.MyInputData.HasOnline) {
				StepDone<HmrcTurnoverAge>().Init(Trail.MyInputData.HasHmrc);
				return;
			} // if

			if (Trail.MyInputData.IsHmrcTurnoverTooOld())
				StepFailed<HmrcTurnoverAge>().Init(Trail.MyInputData.HmrcUpdateTime, Trail.MyInputData.DataAsOf);
			else
				StepDone<HmrcTurnoverAge>().Init(Trail.MyInputData.HmrcUpdateTime, Trail.MyInputData.DataAsOf);

			if (Trail.MyInputData.IsHmrcTurnoverGood(3))
				StepDone<HmrcThreeMonthsTurnover>().Init(Trail.MyInputData.HmrcTurnover3M, Trail.MyInputData.HmrcTurnover1Y, Trail.MyInputData.Configuration.HmrcTurnoverDropQuarterRatio);
			else
				StepFailed<HmrcThreeMonthsTurnover>().Init(Trail.MyInputData.HmrcTurnover3M, Trail.MyInputData.HmrcTurnover1Y, Trail.MyInputData.Configuration.HmrcTurnoverDropQuarterRatio);

			if (Trail.MyInputData.IsHmrcTurnoverGood(6))
				StepDone<HalfYearTurnover>().Init(Trail.MyInputData.HmrcTurnover6M, Trail.MyInputData.HmrcTurnover1Y, Trail.MyInputData.Configuration.HmrcTurnoverDropHalfYearRatio);
			else
				StepFailed<HalfYearTurnover>().Init(Trail.MyInputData.HmrcTurnover6M, Trail.MyInputData.HmrcTurnover1Y, Trail.MyInputData.Configuration.HmrcTurnoverDropHalfYearRatio);
		} // HmrcTurnovers

		#endregion method HmrcTurnovers

		#region method CompanyAge

		private void CompanyAge() {
			if (Trail.MyInputData.MarketplaceSeniority >= Trail.MyInputData.Configuration.MinMPSeniorityDays)
				StepDone<MarketplaceSeniority>().Init(Trail.MyInputData.MarketplaceSeniority, Trail.MyInputData.Configuration.MinMPSeniorityDays);
			else
				StepFailed<MarketplaceSeniority>().Init(Trail.MyInputData.MarketplaceSeniority, Trail.MyInputData.Configuration.MinMPSeniorityDays);
		} // CompanyAge

		#endregion method CompanyAge

		#region method DefaultAccounts

		private void DefaultAccounts() {
			if (Trail.MyInputData.MetaData.NumOfDefaultAccounts > 0)
				StepFailed<DefaultAccounts>().Init(Trail.MyInputData.MetaData.NumOfDefaultAccounts);
			else
				StepDone<DefaultAccounts>().Init(Trail.MyInputData.MetaData.NumOfDefaultAccounts);
		} // DefaultAccounts

		#endregion method DefaultAccounts

		#region method TotalLoanCount

		private void TotalLoanCount() {
			StepDone<TotalLoanCount>().Init(Trail.MyInputData.MetaData.TotalLoanCount);
		} // TotalLoanCount

		#endregion method TotalLoanCount

		#region method CaisStatuses

		private void CaisStatuses(List<string> oAllowedStatuses) {
			List<string> diff = Trail.MyInputData.WorstStatusList.Except(oAllowedStatuses).ToList();

			if (diff.Count > 1)
				StepFailed<WorstCaisStatus>().Init(diff, Trail.MyInputData.WorstStatusList, oAllowedStatuses);
			else
				StepDone<WorstCaisStatus>().Init(null, Trail.MyInputData.WorstStatusList, oAllowedStatuses);
		} // CaisStatuses

		#endregion method CaisStatuses

		#region method Rollovers

		private void Rollovers() {
			if (Trail.MyInputData.MetaData.NumOfRollovers > 0)
				StepFailed<Rollovers>().Init();
			else
				StepDone<Rollovers>().Init();
		} // Rollovers

		#endregion method Rollovers

		#region method LatePayments

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

		#endregion method LatePayments

		#region method CustomerOpenLoans

		private void CustomerOpenLoans() {
			if (Trail.MyInputData.MetaData.OpenLoanCount > Trail.MyInputData.Configuration.MaxNumOfOutstandingLoans)
				StepFailed<OutstandingLoanCount>().Init(Trail.MyInputData.MetaData.OpenLoanCount, Trail.MyInputData.Configuration.MaxNumOfOutstandingLoans);
			else
				StepDone<OutstandingLoanCount>().Init(Trail.MyInputData.MetaData.OpenLoanCount, Trail.MyInputData.Configuration.MaxNumOfOutstandingLoans);
		} // CustomerOpenLoans

		#endregion method CustomerOpenLoans

		#region method RepaidRatio

		private void RepaidRatio() {
			if (Trail.MyInputData.MetaData.RepaidRatio >= Trail.MyInputData.Configuration.MinRepaidPortion)
				StepDone<OutstandingRepayRatio>().Init(Trail.MyInputData.MetaData.RepaidRatio, Trail.MyInputData.Configuration.MinRepaidPortion);
			else
				StepFailed<OutstandingRepayRatio>().Init(Trail.MyInputData.MetaData.RepaidRatio, Trail.MyInputData.Configuration.MinRepaidPortion);
		} // RepaidRatio

		#endregion method RepaidRatio

		#region method ReduceOutstandingPrincipal

		private void ReduceOutstandingPrincipal() {
			ApprovedAmount -= Trail.MyInputData.MetaData.OutstandingPrincipal;

			StepDone<ReduceOutstandingPrincipal>().Init(Trail.MyInputData.MetaData.OutstandingPrincipal, ApprovedAmount);
		} // ReduceOutstandingPrincipal

		#endregion method ReduceOutstandingPrincipal

		#region method AllowedRange

		private void AllowedRange() {
			decimal nApprovedAmount = ApprovedAmount;

			if (Trail.MyInputData.Configuration.IsSilent) {
				StepDone<AmountOutOfRangle>().Init(nApprovedAmount, Trail.MyInputData.Configuration.MinAmount, Trail.MyInputData.Configuration.MaxAmount);
				return;
			} // if

			if ((Trail.MyInputData.Configuration.MinAmount <= nApprovedAmount) && (nApprovedAmount <= Trail.MyInputData.Configuration.MaxAmount))
				StepDone<AmountOutOfRangle>().Init(nApprovedAmount, Trail.MyInputData.Configuration.MinAmount, Trail.MyInputData.Configuration.MaxAmount);
			else
				StepFailed<AmountOutOfRangle>().Init(nApprovedAmount, Trail.MyInputData.Configuration.MinAmount, Trail.MyInputData.Configuration.MaxAmount);
		} // AllowedRange

		#endregion method AllowedRange

		#region method Complete

		private void Complete() {
			decimal nApprovedAmount = ApprovedAmount;

			if (nApprovedAmount > 0)
				StepDone<Complete>().Init(nApprovedAmount);
			else
				StepFailed<Complete>().Init(nApprovedAmount);
		} // Complete

		#endregion method Complete

		#endregion steps

		#region property Trail

		private ApprovalTrail Trail {
			get { return m_oAgent.Trail; }
		} // Trail

		#endregion property Trail

		#region property ApprovedAmount

		private decimal ApprovedAmount {
			get { return m_oAgent.ApprovedAmount; }
			set { m_oAgent.ApprovedAmount = value; }
		} // ApprovedAmount

		#endregion property ApprovedAmount

		#region property Now

		private DateTime Now {
			get { return m_oAgent.Now; }
		} // Now

		#endregion property Now

		#region method StepDone

		private T StepDone<T>() where T : ATrace {
			return Trail.Affirmative<T>(false);
		} // StepFailed

		#endregion method StepDone

		private readonly Agent m_oAgent;

		#endregion private
	} // class Checker
} // namespace
