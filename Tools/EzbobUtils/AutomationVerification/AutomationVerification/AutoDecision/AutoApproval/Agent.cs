namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Agent {
		#region public

		#region constructor

		public Agent(int nCustomerID, decimal nSystemCalculatedAmount, AConnection oDB, ASafeLog oLog) {
			Now = DateTime.UtcNow;
			ApprovedAmount = 0;

			m_oDB = oDB;
			m_oLog = oLog;
			m_oArgs = new Arguments(nCustomerID, nSystemCalculatedAmount);

			m_oMetaData = new MetaData();
			m_oPayments = new List<Payment>();

			m_oFunds = new AvailableFunds();

			m_oTrail = new Trail(m_oArgs.CustomerID);
			m_oCfg = new Configuration(m_oDB, m_oLog);
		} // constructor

		#endregion constructor

		#region method MakeDecision

		// public void MakeDecision(AutoDecisionResponse response) {
		public void MakeDecision() {
			m_oLog.Debug("Checking if auto approval should take place for customer {0}...", m_oArgs.CustomerID);

			ApprovedAmount = m_oArgs.SystemCalculatedAmount;

			try {
				m_oCfg.Load();

				m_oDB.ForEachRowSafe(
					ProcessRow,
					"LoadAutoApprovalData",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", m_oArgs.CustomerID),
					new QueryParameter("Now", Now)
				);

				// TODO: load consumer score and director score using separate stored procedures:
				// GetExperianConsumerScore and GetExperianMinMaxConsumerDirectorsScore

				// TODO: load data for turnovers

				// TODO: load data for seniority

				// TODO: load worst CAIS statuses

				// TODO: load data for after "if has loans".

				m_oDB.GetFirst("GetAvailableFunds", CommandSpecies.StoredProcedure).Fill(m_oFunds);

				m_oMetaData.Validate();

				if ((ApprovedAmount > 0) && (m_oMetaData.ValidationErrors.Count == 0))
					StepDone<InitialAssignment>().Init(ApprovedAmount, m_oMetaData.ValidationErrors);
				else
					StepFailed<InitialAssignment>().Init(ApprovedAmount, m_oMetaData.ValidationErrors);

				CheckIsBrokerCustomer();
				CheckTodayApprovedCount();
				CheckTodayOpenLoans();
				CheckOutstandingOffers();
				CheckAml();
				CheckCustomerStatus();
				CheckCompanyScore();
				// TODO CheckConsumerScore();
				CheckCustomerAge();
				/*
				CheckTurnovers();
				CheckCompanyAge();
				CheckDefaultAccounts();

				if (customer has loans) {
					CheckCaisStatuses(m_oCfg.GetAllowedCaisStatusesWithLoan());
					CheckRollovers();
				*/
					CheckLatePayments();
				/*
					CheckCustomerOpenLoans();
					CheckRepaidRatio();
					ReduceOutstandingPrincipal();
				}
				else
					CheckCaisStatuses(m_oCfg.GetAllowedCaisStatusesWithoutLoan());

				CheckAllowedRange();

				if (m_oTrail.IsApproved) {
					response.AutoApproveAmount = (int)ApprovedAmount;
					response.CreditResult = "Approved";
					response.UserStatus = "Approved";
					response.SystemDecision = "Approve";
					response.LoanOfferUnderwriterComment = "Auto Approval";
					response.DecisionName = "Approval";
					response.AppValidFor = Now.AddDays(m_oMetaData.OfferLength);
					response.IsAutoApproval = true;
					response.LoanOfferEmailSendingBannedNew = m_oMetaData.IsEmailSendingBanned;
				} // if
				*/
			}
			catch (Exception e) {
				m_oLog.Error(e, "Exception during auto approval.");
				StepFailed<ExceptionThrown>().Init(e);
			} // try

			decimal nApprovedAmount = ApprovedAmount;
			if (nApprovedAmount > 0)
				StepDone<Complete>().Init(nApprovedAmount);
			else
				StepFailed<Complete>().Init(nApprovedAmount);

			m_oLog.Debug("Checking if auto approval should take place for customer {0} complete.", m_oArgs.CustomerID);

			m_oLog.Msg("Auto approved amount: {0}. {1}", ApprovedAmount, m_oTrail);
		} // MakeDecision

		#endregion method MakeDecision

		public decimal ApprovedAmount { get; private set; }

		#endregion public

		#region private

		#region steps

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
			if (m_oMetaData.TodayAutoApprovalCount > m_oCfg.MaxDailyApprovals)
				StepFailed<TodayApprovalCount>().Init(m_oMetaData.TodayAutoApprovalCount, m_oCfg.MaxDailyApprovals);
			else
				StepDone<TodayApprovalCount>().Init(m_oMetaData.TodayAutoApprovalCount, m_oCfg.MaxDailyApprovals);
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

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		#endregion method ProcessRow

		#region enum RowType

		private enum RowType {
			MetaData,
			Payment,
		} // enum RowType

		#endregion enum RowType

		#region method StepFailed

		private T StepFailed<T>() where T : ATrace {
			ApprovedAmount = 0;
			return m_oTrail.Failed<T>();
		} // StepFailed

		#endregion method StepFailed

		#region method StepDone

		private T StepDone<T>() where T : ATrace {
			return m_oTrail.Done<T>();
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

		private readonly AvailableFunds m_oFunds;

		private readonly Trail m_oTrail;

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
