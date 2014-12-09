namespace Ezbob.Backend.Strategies.MainStrategy.AutoDecisions
{
	using System;
	using AutomationCalculator.AutoDecision.AutoReRejection;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoReRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ReRejection
	{
		private readonly AConnection Db;
		private readonly ASafeLog log;
		private readonly int customerId;
		private readonly ReRejectionTrail m_oTrail;

		public ReRejection(int customerId, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog ?? new SafeLog();
			this.customerId = customerId;
			m_oTrail = new ReRejectionTrail(
				customerId,
				oLog,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);
		}

		public bool MakeAndVerifyDecision() {
			RunPrimary();

			Agent oSecondary = RunSecondary();

			bool bSuccess = m_oTrail.EqualsTo(oSecondary.Trail);

			m_oTrail.Save(Db, oSecondary.Trail);

			return bSuccess;
		} // MakeAndVerifyDecision

		public void MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				if (MakeAndVerifyDecision() && m_oTrail.HasDecided)
				{
					response.Decision = DecisionActions.ReReject;
					response.AutoRejectReason = "Auto Re-Reject";
					response.CreditResult = CreditResultStatus.Rejected;
					response.UserStatus = Status.Rejected;
					response.SystemDecision = SystemDecision.Reject;
					response.DecisionName = "Re-rejection";
				}
			}
			catch (Exception ex)
			{
				StepNoReReject<ExceptionThrown>(true).Init(ex);
				log.Error(ex, "Exception during re-rejection {0}", m_oTrail);
			}
		}

		private void RunPrimary() {
			log.Debug("Primary: checking if auto re-reject should take place for customer {0}...", customerId);

			SafeReader sr = Db.GetFirst("GetCustomerDataForReRejection", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

			bool lastDecisionWasReject = sr["LastDecisionWasReject"];
			DateTime? lastRejectDate = sr["LastRejectDate"]; 
			DateTime? lastDecisionDate = sr["LastDecisionDate"]; 
			bool newDataSourceAdded = sr["NewDataSourceAdded"];
			int openLoansAmount = sr["OpenLoansAmount"];
			decimal principalRepaymentAmount = sr["PrincipalRepaymentAmount"];
			int numOfOpenLoans = sr["NumOfOpenLoans"];

			m_oTrail.MyInputData.Init(DateTime.UtcNow,
				lastDecisionWasReject,
				lastRejectDate,
				lastDecisionDate,
				newDataSourceAdded,
				openLoansAmount,
				principalRepaymentAmount,
				numOfOpenLoans,
				CurrentValues.Instance.AutoReRejectMinRepaidPortion,
				CurrentValues.Instance.AutoReRejectMaxLRDAge,
				CurrentValues.Instance.AutoReRejectMaxAllowedLoans
			);

			CheckNumOfOpenLoans();
			CheckLastDecisionWasReject();
			CheckNewMarketPlaceAdded();
			CheckLRDIsTooOld();
			CheckOpenLoansRepayments();

			log.Debug(
				"Primary: checking if auto re-reject should take place for customer {0} complete; {1}", customerId, m_oTrail
			);
		} // RunPrimary

		private AutomationCalculator.AutoDecision.AutoReRejection.Agent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(
				customerId, m_oTrail.InputData.DataAsOf, Db, log
			).Init();

			oSecondary.MakeDecision();

			return oSecondary;
		} // RunSecondary

		private void CheckLastDecisionWasReject()
		{
			if (!m_oTrail.MyInputData.LastDecisionWasReject)
			{
				StepNoReReject<LastDecisionWasReject>(bLockDecisionAfterAddingAStep: true).Init(m_oTrail.MyInputData.LastDecisionWasReject);
			}
			else
			{
				StepNoDecision<LastDecisionWasReject>().Init(m_oTrail.MyInputData.LastDecisionWasReject);
			}
		}

		private void CheckNewMarketPlaceAdded()
		{
			if (m_oTrail.MyInputData.LastDecisionDate.HasValue && m_oTrail.MyInputData.NewDataSourceAdded)
			{
				StepNoReReject<MarketPlaceWasAdded>(bLockDecisionAfterAddingAStep: true).Init(m_oTrail.MyInputData.NewDataSourceAdded);
			}
			else
			{
				StepNoDecision<MarketPlaceWasAdded>().Init(m_oTrail.MyInputData.NewDataSourceAdded);
			}
		}

		private void CheckLRDIsTooOld()
		{
			if (m_oTrail.MyInputData.LastRejectDate.HasValue && (decimal)(m_oTrail.MyInputData.DataAsOf - m_oTrail.MyInputData.LastRejectDate.Value).TotalDays > m_oTrail.MyInputData.AutoReRejectMaxLRDAge)
			{
				StepNoReReject<LRDIsTooOld>(bLockDecisionAfterAddingAStep: true).Init((decimal)(m_oTrail.MyInputData.DataAsOf - m_oTrail.MyInputData.LastRejectDate.Value).TotalDays, m_oTrail.MyInputData.AutoReRejectMaxLRDAge);
			}
			else
			{
				var days = m_oTrail.MyInputData.LastRejectDate.HasValue ? (decimal)(m_oTrail.MyInputData.DataAsOf - m_oTrail.MyInputData.LastRejectDate.Value).TotalDays : 0.0M;
				StepNoDecision<LRDIsTooOld>().Init(days, m_oTrail.MyInputData.AutoReRejectMaxLRDAge);
			}

		}

		private void CheckNumOfOpenLoans()
		{
			if (m_oTrail.MyInputData.NumOfOpenLoans >= m_oTrail.MyInputData.AutoReRejectMaxAllowedLoans)
			{
				StepReReject<OpenLoans>(bLockDecisionAfterAddingAStep: true).Init(m_oTrail.MyInputData.NumOfOpenLoans, m_oTrail.MyInputData.AutoReRejectMaxAllowedLoans);
			}
			else
			{
				StepNoDecision<OpenLoans>().Init(m_oTrail.MyInputData.NumOfOpenLoans, m_oTrail.MyInputData.AutoReRejectMaxAllowedLoans);
			}
		}

		private void CheckOpenLoansRepayments() {
			decimal ratio = m_oTrail.MyInputData.OpenLoansAmount == 0 ? 0 : m_oTrail.MyInputData.PrincipalRepaymentAmount / m_oTrail.MyInputData.OpenLoansAmount;
			//no open loans
			if (m_oTrail.MyInputData.OpenLoansAmount == 0) {
				StepNoDecision<OpenLoansRepayments>().Init(m_oTrail.MyInputData.OpenLoansAmount, 0, 0);
			}
			else {
				if (ratio < m_oTrail.MyInputData.AutoReRejectMinRepaidPortion) {
					StepReReject<OpenLoansRepayments>(true).Init(
						m_oTrail.MyInputData.OpenLoansAmount,
						m_oTrail.MyInputData.PrincipalRepaymentAmount,
						m_oTrail.MyInputData.AutoReRejectMinRepaidPortion
						);
				}
				else {
					StepNoReReject<OpenLoansRepayments>(true).Init(
						m_oTrail.MyInputData.OpenLoansAmount,
						m_oTrail.MyInputData.PrincipalRepaymentAmount,
						m_oTrail.MyInputData.AutoReRejectMinRepaidPortion
						);
				}
			}
		}

		private T StepNoReReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace
		{
			return m_oTrail.Negative<T>(bLockDecisionAfterAddingAStep);
		}

		private T StepReReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace
		{
			return m_oTrail.Affirmative<T>(bLockDecisionAfterAddingAStep);
		}

		private T StepNoDecision<T>() where T : ATrace
		{
			return m_oTrail.Dunno<T>();
		}
	}
}
