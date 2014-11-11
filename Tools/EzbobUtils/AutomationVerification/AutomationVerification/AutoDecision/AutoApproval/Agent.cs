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
			ApprovedAmount = 0;

			m_oDB = oDB;
			m_oLog = oLog;
			m_oArgs = new Arguments(nCustomerID, nSystemCalculatedAmount);

			m_oMetaData = new MetaData();
			m_oPayments = new List<Payment>();

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
					new QueryParameter("Now", DateTime.UtcNow)
				);

				// TODO: load consumer score and director score using separate stored procedures:
				// GetExperianConsumerScore and GetExperianMinMaxConsumerDirectorsScore

				// TODO: load data for turnovers

				// TODO: load data for seniority

				// TODO: load worst CAIS statuses

				// TODO: load data for after "if has loans".

				m_oMetaData.Validate();

				if ((ApprovedAmount > 0) && (m_oMetaData.ValidationErrors.Count == 0))
					StepDone<InitialAssignment>().Init(ApprovedAmount, m_oMetaData.ValidationErrors);
				else
					StepFailed<InitialAssignment>().Init(ApprovedAmount, m_oMetaData.ValidationErrors);

				/*
				CheckIsBrokerCustomer();
				CheckTodayApprovedCount();
				CheckTodayOpenLoans();
				CheckOutstandingOffers(); // ReservedAmount from available funds
				CheckAml();
				CheckCustomerStatus();
				CheckCompanyScore();
				CheckConsumerScore();
				CheckCustomerAge();
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
					response.AppValidFor = DateTime.UtcNow.AddDays(m_oMetaData.OfferLength);
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
				StepDone<LatePayment>().Init(0, 0, DateTime.UtcNow, 0, DateTime.UtcNow, m_oCfg.MaxAllowedDaysLate);
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

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private readonly Configuration m_oCfg;
		private readonly Arguments m_oArgs;

		private readonly MetaData m_oMetaData;
		private readonly List<Payment> m_oPayments;

		private readonly Trail m_oTrail;

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
