namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.ReApproval;
	using AutomationCalculator.ProcessHistory.Trails;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Agent {
		#region public

		#region constructor

		public Agent(int nCustomerID, AConnection oDB, ASafeLog oLog) {
			m_oMetaData = new MetaData();
			m_oLatePayments = new List<Payment>();
			m_oNewMarketplaces = new List<Marketplace>();
			ApprovedAmount = 0;

			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();
			m_oArgs = new Arguments(nCustomerID);

			m_oTrail = new ReapprovalTrail(m_oArgs.CustomerID, m_oLog);
			m_oCfg = new Configuration(m_oDB, m_oLog);
		} // constructor

		#endregion constructor

		#region method MakeDecision

		public void MakeDecision(AutoDecisionResponse response) {
			m_oLog.Debug("Checking if auto re-approval should take place for customer {0}...", m_oArgs.CustomerID);

			try {
				m_oCfg.Load();

				m_oDB.ForEachRowSafe(
					ProcessRow,
					"LoadAutoReapprovalData",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", m_oArgs.CustomerID)
				);

				m_oMetaData.Validate();

				if (m_oMetaData.ValidationErrors.Count == 0)
					StepDone<InitialAssignment>().Init(m_oMetaData.ValidationErrors);
				else
					StepFailed<InitialAssignment>().Init(m_oMetaData.ValidationErrors);

				CheckIsFraud();
				CheckLacrTooOld();
				CheckRejectAfterLacr();
				CheckLateLoans();
				CheckLatePayments();
				CheckNewMarketplaces();
				CheckOutstandingLoans();
				CheckLoanCharges();
				SetApprovedAmount();
				CheckAvailableFunds();

				if (m_oTrail.HasDecided) {
					response.AutoApproveAmount = (int)ApprovedAmount;
					response.IsAutoApproval = true;
					response.CreditResult = "Approved";
					response.UserStatus = "Approved";
					response.SystemDecision = "Approve";
					response.LoanOfferUnderwriterComment = "Auto Re-Approval";
					response.DecisionName = "Re-Approval";
					response.AppValidFor = DateTime.UtcNow.AddDays(m_oMetaData.OfferLength);
					response.LoanOfferEmailSendingBannedNew = m_oMetaData.IsEmailSendingBanned;
				} // if
			}
			catch (Exception e) {
				m_oLog.Error(e, "Exception during re-approval.");
				StepFailed<ExceptionThrown>().Init(e);
			} // try

			decimal nApprovedAmount = ApprovedAmount;
			if (nApprovedAmount > 0)
				StepDone<Complete>().Init(nApprovedAmount);
			else
				StepFailed<Complete>().Init(nApprovedAmount);

			m_oLog.Debug("Checking if auto re-approval should take place for customer {0} complete.", m_oArgs.CustomerID);

			m_oLog.Msg("Auto re-approved amount: {0}. {1}", ApprovedAmount, m_oTrail);
		} // MakeDecision

		#endregion method MakeDecision

		public decimal ApprovedAmount { get; private set; }

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

		#region method CheckLacrTooOld

		private void CheckLacrTooOld() {
			if (m_oMetaData.LacrIsTooOld(m_oCfg.MaxLacrAge))
				StepFailed<LacrTooOld>().Init(m_oMetaData.LacrAge ?? -1, m_oCfg.MaxLacrAge);
			else
				StepDone<LacrTooOld>().Init(m_oMetaData.LacrAge ?? -1, m_oCfg.MaxLacrAge);
		} // CheckLacrTooOld

		#endregion method CheckLacrTooOld

		#region method CheckRejectAfterLacr

		private void CheckRejectAfterLacr() {
			if (m_oMetaData.RejectAfterLacrID > 0)
				StepFailed<RejectAfterLacr>().Init(m_oMetaData.RejectAfterLacrID, m_oMetaData.LacrID);
			else
				StepDone<RejectAfterLacr>().Init(m_oMetaData.RejectAfterLacrID, m_oMetaData.LacrID);
		} // CheckRejectAfterLacr

		#endregion method CheckRejectAfterLacr

		#region method CheckLateLoans

		private void CheckLateLoans() {
			if (m_oMetaData.LateLoanCount > 0)
				StepFailed<LateLoans>().Init();
			else
				StepDone<LateLoans>().Init();
		} // CheckLateLoans

		#endregion method CheckLateLoans

		#region method CheckLatePayments

		private void CheckLatePayments() {
			bool bHasLatePayments = false;

			foreach (Payment lp in m_oLatePayments) {
				if (lp.IsLate(m_oCfg.MaxLatePayment)) {
					bHasLatePayments = true;

					lp.Fill(StepFailed<LatePayment>(), m_oCfg.MaxLatePayment);
				} // if
			} // for each

			if (!bHasLatePayments)
				StepDone<LatePayment>().Init(0, 0, DateTime.UtcNow, 0, DateTime.UtcNow, m_oCfg.MaxLatePayment);
		} // CheckLatePayments

		#endregion method CheckLatePayments

		#region method CheckNewMarketplaces

		private void CheckNewMarketplaces() {
			if (m_oNewMarketplaces.Count < 1)
				StepDone<NewMarketplace>().Init(0, string.Empty, string.Empty, DateTime.UtcNow);
			else
				foreach (var mp in m_oNewMarketplaces)
					mp.Fill(StepFailed<NewMarketplace>());
		} // CheckNewMarketplaces

		#endregion method CheckNewMarketplaces

		#region method CheckOutstandingLoans

		private void CheckOutstandingLoans() {
			if (m_oMetaData.OpenLoanCount > m_oCfg.MaxNumOfOutstandingLoans)
				StepFailed<OutstandingLoanCount>().Init(m_oMetaData.OpenLoanCount, m_oCfg.MaxNumOfOutstandingLoans);
			else
				StepDone<OutstandingLoanCount>().Init(m_oMetaData.OpenLoanCount, m_oCfg.MaxNumOfOutstandingLoans);
		} // CheckOutstandingLoans

		#endregion method CheckOutstandingLoans

		#region method CheckLoanCharges

		private void CheckLoanCharges() {
			if (m_oMetaData.SumOfCharges > 0)
				StepFailed<Charges>().Init(m_oMetaData.SumOfCharges);
			else
				StepDone<Charges>().Init(0);
		} // CheckLoanCharges

		#endregion method CheckLoanCharges

		#region method SetApprovedAmount

		private void SetApprovedAmount() {
			if (m_oTrail.HasDecided)
				ApprovedAmount = m_oMetaData.ApprovedAmount;

			if (ApprovedAmount > 0)
				StepDone<ApprovedAmount>().Init(ApprovedAmount);
			else
				StepFailed<ApprovedAmount>().Init(ApprovedAmount);
		} // SetApprovedAmount

		#endregion method SetApprovedAmount

		#region method CheckAvailableFunds

		private void CheckAvailableFunds() {
			decimal nApprovedAmount = ApprovedAmount;

			var availFunds = new GetAvailableFunds(m_oDB, m_oLog);
			availFunds.Execute();

			var nAvailFunds = availFunds.AvailableFunds - availFunds.ReservedAmount;

			if (nApprovedAmount < nAvailFunds)
				StepDone<EnoughFunds>().Init(nApprovedAmount, nAvailFunds);
			else
				StepFailed<EnoughFunds>().Init(nApprovedAmount, nAvailFunds);
		} // CheckAvailableFunds

		#endregion method CheckAvailableFunds

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

			case RowType.LatePayment:
				m_oLatePayments.Add(sr.Fill<Payment>());
				break;

			case RowType.Marketplace:
				m_oNewMarketplaces.Add(sr.Fill<Marketplace>());
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		#endregion method ProcessRow

		#region enum RowType

		private enum RowType {
			MetaData,
			LatePayment,
			Marketplace,
		} // enum RowType

		#endregion enum RowType

		#region method StepFailed

		private T StepFailed<T>() where T : ATrace {
			ApprovedAmount = 0;
			return m_oTrail.Negative<T>();
		} // StepFailed

		#endregion method StepFailed

		#region method StepDone

		private T StepDone<T>() where T : ATrace {
			return m_oTrail.Affirmative<T>();
		} // StepFailed

		#endregion method StepDone

		#region fields

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private readonly Configuration m_oCfg;
		private readonly Arguments m_oArgs;
		private readonly MetaData m_oMetaData;

		private readonly List<Payment> m_oLatePayments;

		private readonly List<Marketplace> m_oNewMarketplaces;

		private readonly ReapprovalTrail m_oTrail;

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
