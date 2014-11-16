namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using AutomationCalculator.ProcessHistory.Trails;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Agent {
		#region public

		#region constructor

		public Agent(int nCustomerID, decimal nSystemCalculatedAmount, AConnection oDB, ASafeLog oLog) {
			Result = null;
			Now = DateTime.UtcNow;
			m_nApprovedAmount = 0;

			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();
			m_oArgs = new Arguments(nCustomerID, nSystemCalculatedAmount);

			m_oMetaData = new MetaData();
			m_oPayments = new List<Payment>();

			m_oFunds = new AvailableFunds();
			m_oWorstStatuses = new SortedSet<string>();

			m_oTurnover = new CalculatedTurnover();

			m_oOriginationTime = new OriginationTime();

			Trail = new ApprovalTrail(m_oArgs.CustomerID, m_oLog);
			m_oCfg = new Configuration(m_oDB, m_oLog);
		} // constructor

		#endregion constructor

		#region method MakeDecision

		// public void MakeDecision(AutoDecisionResponse response) {
		public void MakeDecision() {
			m_oLog.Debug("Checking if auto approval should take place for customer {0}...", m_oArgs.CustomerID);

			m_nApprovedAmount = m_oArgs.SystemCalculatedAmount;

			try {
				m_oCfg.Load();

				m_oDB.ForEachRowSafe(
					ProcessRow,
					"LoadAutoApprovalData",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", m_oArgs.CustomerID),
					new QueryParameter("Now", Now)
				);

				m_oOriginationTime.FromExperian(m_oMetaData.IncorporationDate);

				m_oDB.GetFirst("GetAvailableFunds", CommandSpecies.StoredProcedure).Fill(m_oFunds);

				m_oMetaData.Validate();

				// Once a step is not passed there is no need to continue result-wise. However the
				// process continues because we want to pick all the possible reasons for not
				// approving a customer in order to compare different implementations of the process.

				if ((m_nApprovedAmount > 0) && (m_oMetaData.ValidationErrors.Count == 0))
					StepDone<InitialAssignment>().Init(m_nApprovedAmount, m_oMetaData.ValidationErrors);
				else
					StepFailed<InitialAssignment>().Init(m_nApprovedAmount, m_oMetaData.ValidationErrors);

				CheckIsFraud();
				CheckIsBrokerCustomer();
				CheckTodayApprovedCount();
				CheckTodayOpenLoans();
				CheckOutstandingOffers();
				CheckAml();
				CheckCustomerStatus();
				CheckCompanyScore();
				CheckConsumerScore();
				CheckCustomerAge();
				CheckTurnovers();
				CheckCompanyAge();
				CheckDefaultAccounts();

				StepDone<TotalLoanCount>().Init(m_oMetaData.TotalLoanCount);

				CheckCaisStatuses(m_oMetaData.TotalLoanCount > 0
					? m_oCfg.GetAllowedCaisStatusesWithLoan()
					: m_oCfg.GetAllowedCaisStatusesWithoutLoan()
				);

				CheckRollovers();
				CheckLatePayments();
				CheckCustomerOpenLoans();
				CheckRepaidRatio();
				ReduceOutstandingPrincipal();

				CheckAllowedRange();

				decimal nApprovedAmount = m_nApprovedAmount;
				if (nApprovedAmount > 0)
					StepDone<Complete>().Init(nApprovedAmount);
				else
					StepFailed<Complete>().Init(nApprovedAmount);
			}
			catch (Exception e) {
				m_oLog.Error(e, "Exception during auto approval.");
				StepFailed<ExceptionThrown>().Init(e);
			} // try

			if (Trail.HasDecided)
				Result = new Result((int)m_nApprovedAmount, (int)m_oMetaData.OfferLength, m_oMetaData.IsEmailSendingBanned);

			m_oLog.Debug(
				"Checking if auto approval should take place for customer {0} complete; {1}\n{2}",
				m_oArgs.CustomerID,
				Trail,
				Result == null ? string.Empty : "Approved " + Result + "."
			);
		} // MakeDecision

		#endregion method MakeDecision

		public Result Result { get; private set; }

		public ApprovalTrail Trail { get; private set; }

		#endregion public

		#region private

		#region steps

		#region method CheckIsFraud

		private void CheckIsFraud() {
			if (m_oMetaData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(m_oMetaData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(m_oMetaData.FraudStatus);
		} // CheckIsFraud

		#endregion method CheckIsFraud

		#region method CheckIsBrokerCustomer

		private void CheckIsBrokerCustomer() {
			if (m_oMetaData.IsBrokerCustomer)
				StepFailed<IsBrokerCustomer>().Init();
			else
				StepDone<IsBrokerCustomer>().Init();
		} // CheckIsBrokerCustomer

		#endregion method CheckIsBrokerCustomer

		#region method CheckTodayApprovedCount

		private void CheckTodayApprovedCount() {
			if (m_oMetaData.NumOfTodayAutoApproval > m_oCfg.MaxDailyApprovals)
				StepFailed<TodayApprovalCount>().Init(m_oMetaData.NumOfTodayAutoApproval, m_oCfg.MaxDailyApprovals);
			else
				StepDone<TodayApprovalCount>().Init(m_oMetaData.NumOfTodayAutoApproval, m_oCfg.MaxDailyApprovals);
		} // CheckTodayApprovedCount

		#endregion method CheckTodayApprovedCount

		#region method CheckTodayOpenLoans

		private void CheckTodayOpenLoans() {
			if (m_oMetaData.TodayLoanSum > m_oCfg.MaxTodayLoans)
				StepFailed<TodayLoans>().Init(m_oMetaData.TodayLoanSum, m_oCfg.MaxTodayLoans);
			else
				StepDone<TodayLoans>().Init(m_oMetaData.TodayLoanSum, m_oCfg.MaxTodayLoans);
		} // CheckTodayOpenLoans

		#endregion method CheckTodayOpenLoans

		#region method CheckOutstandingOffers

		private void CheckOutstandingOffers() {
			if (m_oFunds.Reserved > m_oCfg.MaxOutstandingOffers)
				StepFailed<OutstandingOffers>().Init(m_oFunds.Reserved, m_oCfg.MaxOutstandingOffers);
			else
				StepDone<OutstandingOffers>().Init(m_oFunds.Reserved, m_oCfg.MaxOutstandingOffers);
		} // CheckOutstandingOffers

		#endregion method CheckOutstandingOffers

		#region method CheckAml

		private void CheckAml() {
			if (0 == string.Compare(m_oMetaData.AmlResult, "passed", StringComparison.InvariantCultureIgnoreCase))
				StepDone<AmlCheck>().Init(m_oMetaData.AmlResult);
			else
				StepFailed<AmlCheck>().Init(m_oMetaData.AmlResult);
		} // CheckAml

		#endregion method CheckAml

		#region method CheckCustomerStatus

		private void CheckCustomerStatus() {
			if (m_oMetaData.CustomerStatusEnabled)
				StepDone<CustomerStatus>().Init(m_oMetaData.CustomerStatusName);
			else
				StepFailed<CustomerStatus>().Init(m_oMetaData.CustomerStatusName);
		} // CheckCustomerStatus

		#endregion method CheckCustomerStatus

		#region method CheckCompanyScore

		private void CheckCompanyScore() {
			if (m_oMetaData.CompanyScore <= 0)
				StepDone<BusinessScore>().Init(m_oMetaData.CompanyScore, m_oCfg.BusinessScoreThreshold);
			else if (m_oMetaData.CompanyScore > m_oCfg.BusinessScoreThreshold)
				StepDone<BusinessScore>().Init(m_oMetaData.CompanyScore, m_oCfg.BusinessScoreThreshold);
			else
				StepFailed<BusinessScore>().Init(m_oMetaData.CompanyScore, m_oCfg.BusinessScoreThreshold);
		} // CheckCompayScore

		#endregion method CheckCompanyScore

		#region method CheckConsumerScore

		private void CheckConsumerScore() {
			if (m_oMetaData.ConsumerScore >= m_oCfg.ExperianScoreThreshold)
				StepDone<ConsumerScore>().Init(m_oMetaData.ConsumerScore, m_oCfg.ExperianScoreThreshold);
			else
				StepFailed<ConsumerScore>().Init(m_oMetaData.ConsumerScore, m_oCfg.ExperianScoreThreshold);
		} // CheckConsumerScore

		#endregion method CheckConsumerScore

		#region method CheckCustomerAge

		private void CheckCustomerAge() {
			// nAge: full number of years in customer's age.
			int nAge = Now.Year - m_oMetaData.DateOfBirth.Year;

			if (m_oMetaData.DateOfBirth.AddYears(nAge) < Now) // this happens if customer had no birthday this year.
				nAge--;

			if ((m_oCfg.CustomerMinAge <= nAge) && (nAge <= m_oCfg.CustomerMaxAge))
				StepDone<Age>().Init(nAge, m_oCfg.CustomerMinAge, m_oCfg.CustomerMaxAge);
			else
				StepFailed<Age>().Init(nAge, m_oCfg.CustomerMinAge, m_oCfg.CustomerMaxAge);
		} // CheckCustomerAge

		#endregion method CheckCustomerAge

		#region method CheckTurnovers

		private void CheckTurnovers() {
			var oThresholds = new SortedDictionary<int, Tuple<decimal, TimePeriodEnum>> {
				{  1, new Tuple<decimal, TimePeriodEnum>(m_oCfg.MinTurnover1M, TimePeriodEnum.Month)  },
				{  3, new Tuple<decimal, TimePeriodEnum>(m_oCfg.MinTurnover3M, TimePeriodEnum.Month3) },
				{ 12, new Tuple<decimal, TimePeriodEnum>(m_oCfg.MinTurnover1Y, TimePeriodEnum.Year)   },
			};

			foreach (var pair in oThresholds) {
				decimal nTurnover = m_oTurnover[pair.Key];
				decimal nThreshold = pair.Value.Item1;
				TimePeriodEnum nPeriod = pair.Value.Item2;

				Turnover oTrace = (nTurnover > nThreshold) ? StepDone<Turnover>() : StepFailed<Turnover>();
				oTrace.PeriodName = nPeriod.ToString();
				oTrace.Init(nTurnover, nThreshold);
			} // for each
		} // CheckTurnovers

		#endregion method CheckTurnovers

		#region method CheckCompanyAge

		private void CheckCompanyAge() {
			m_oLog.Msg("Customer {0} business seniority: {1}.", m_oArgs.CustomerID, m_oOriginationTime);

			if (m_oOriginationTime.Since == null)
				StepFailed<MarketplaceSeniority>().Init(-1, m_oCfg.MinMPSeniorityDays);
			else {
				int nAge = (int)(DateTime.UtcNow - m_oOriginationTime.Since.Value).TotalDays;

				if (nAge >= m_oCfg.MinMPSeniorityDays)
					StepDone<MarketplaceSeniority>().Init(nAge, m_oCfg.MinMPSeniorityDays);
				else
					StepFailed<MarketplaceSeniority>().Init(nAge, m_oCfg.MinMPSeniorityDays);
			} // if
		} // CheckCompanyAge

		#endregion method CheckCompanyAge

		#region method CheckDefaultAccounts

		private void CheckDefaultAccounts() {
			if (m_oMetaData.NumOfDefaultAccounts > 0)
				StepFailed<DefaultAccounts>().Init();
			else
				StepDone<DefaultAccounts>().Init();
		} // CheckDefaultAccounts

		#endregion method CheckDefaultAccounts

		#region method CheckCaisStatuses

		private void CheckCaisStatuses(List<string> oAllowedStatuses) {
			List<string> diff = m_oWorstStatuses.Except(oAllowedStatuses).ToList();

			if (diff.Count > 1)
				StepFailed<WorstCaisStatus>().Init(diff, m_oWorstStatuses.ToList(), oAllowedStatuses);
			else
				StepDone<WorstCaisStatus>().Init(null, m_oWorstStatuses.ToList(), oAllowedStatuses);
		} // CheckCaisStatuses

		#endregion method CheckCaisStatuses

		#region method CheckRollovers

		private void CheckRollovers() {
			if (m_oMetaData.NumOfRollovers > 0)
				StepFailed<Rollovers>().Init();
			else
				StepDone<Rollovers>().Init();
		} // CheckRollovers

		#endregion method CheckRollovers

		#region method CheckLatePayments

		private void CheckLatePayments() {
			bool bHasLatePayments = false;

			foreach (Payment lp in m_oPayments) {
				if (lp.IsLate(m_oCfg.MaxAllowedDaysLate)) {
					bHasLatePayments = true;

					lp.Fill(StepFailed<LatePayment>(), m_oCfg.MaxAllowedDaysLate);
				} // if
			} // for each

			if (!bHasLatePayments)
				StepDone<LatePayment>().Init(0, 0, Now, 0, Now, m_oCfg.MaxAllowedDaysLate);
		} // CheckLatePayments

		#endregion method CheckLatePayments

		#region method CheckCustomerOpenLoans

		private void CheckCustomerOpenLoans() {
			if (m_oMetaData.OpenLoanCount > m_oCfg.MaxNumOfOutstandingLoans)
				StepFailed<OutstandingLoanCount>().Init(m_oMetaData.OpenLoanCount, m_oCfg.MaxNumOfOutstandingLoans);
			else
				StepDone<OutstandingLoanCount>().Init(m_oMetaData.OpenLoanCount, m_oCfg.MaxNumOfOutstandingLoans);
		} // CheckCustomerOpenLoans

		#endregion method CheckCustomerOpenLoans

		#region method CheckRepaidRatio

		private void CheckRepaidRatio() {
			decimal nRatio = m_oMetaData.RepaidRatio;

			if (nRatio > m_oCfg.MinRepaidPortion)
				StepDone<OutstandingRepayRatio>().Init(nRatio, m_oCfg.MinRepaidPortion);
			else
				StepFailed<OutstandingRepayRatio>().Init(nRatio, m_oCfg.MinRepaidPortion);
		} // CheckRepaidRatio

		#endregion method CheckRepaidRatio

		#region method ReduceOutstandingPrincipal

		private void ReduceOutstandingPrincipal() {
			m_nApprovedAmount -= m_oMetaData.OutstandingPrincipal;
		} // ReduceOutstandingPrincipal

		#endregion method ReduceOutstandingPrincipal

		#region method CheckAllowedRange

		private void CheckAllowedRange() {
			decimal nApprovedAmount = m_nApprovedAmount;

			if ((m_oCfg.MinAmount <= nApprovedAmount) && (nApprovedAmount <= m_oCfg.MaxAmount))
				StepDone<AmountOutOfRangle>().Init(nApprovedAmount, m_oCfg.MinAmount, m_oCfg.MaxAmount);
			else
				StepFailed<AmountOutOfRangle>().Init(nApprovedAmount, m_oCfg.MinAmount, m_oCfg.MaxAmount);
		} // CheckAllowedRange

		#endregion method CheckAllowedRange

		#endregion steps

		#region method ProcessRow

		private void ProcessRow(SafeReader sr) {
			RowType nRowType;

			string sRowType = sr["RowType"];

			if (!Enum.TryParse(sRowType, out nRowType)) {
				m_oLog.Alert("Unsupported row type encountered: '{0}'.", sRowType);
				return;
			} // if

			switch (nRowType) {
			case RowType.MetaData:
				sr.Fill(m_oMetaData);
				break;

			case RowType.Payment:
				m_oPayments.Add(sr.Fill<Payment>());
				break;

			case RowType.Cais:
				m_oWorstStatuses.Add(sr["WorstStatus"]);
				break;

			case RowType.OriginationTime:
				m_oOriginationTime.Process(sr);
				break;

			case RowType.Turnover:
				m_oTurnover.Add(sr, m_oLog);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		#endregion method ProcessRow

		#region enum RowType

		private enum RowType {
			MetaData,
			Payment,
			Cais,
			OriginationTime,
			Turnover,
		} // enum RowType

		#endregion enum RowType

		#region method StepFailed

		private T StepFailed<T>() where T : ATrace {
			m_nApprovedAmount = 0;
			return Trail.Dunno<T>();
		} // StepFailed

		#endregion method StepFailed

		#region method StepDone

		private T StepDone<T>() where T : ATrace {
			return Trail.Affirmative<T>();
		} // StepFailed

		#endregion method StepDone

		#region fields

		private readonly DateTime Now;

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private readonly Configuration m_oCfg;
		private readonly Arguments m_oArgs;

		private readonly MetaData m_oMetaData;
		private readonly List<Payment> m_oPayments;

		private readonly SortedSet<string> m_oWorstStatuses;

		private readonly OriginationTime m_oOriginationTime;

		private decimal m_nApprovedAmount;

		private readonly CalculatedTurnover m_oTurnover;

		private readonly AvailableFunds m_oFunds;

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
