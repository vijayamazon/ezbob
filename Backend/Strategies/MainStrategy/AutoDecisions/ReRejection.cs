namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions
{
	using System;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoReRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
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
			m_oTrail = new ReRejectionTrail(customerId, oLog, CurrentValues.Instance.AutomationExplanationMailReciever, CurrentValues.Instance.MailSenderEmail, CurrentValues.Instance.MailSenderName);
		}

		public bool MakeAndVerifyDecision() {
			#region primary

			log.Debug("Primary: checking if auto re-reject should take place for customer {0}...", customerId);

			SafeReader sr = Db.GetFirst("GetCustomerDataForReRejection", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

			bool wasManuallyRejected = sr["WasManuallyRejected"];
			DateTime? lastManualRejectDate = sr["LastManualRejectDate"];
			bool newDataSourceAdded = sr["NewDataSourceAdded"];
			int openLoansAmount = sr["OpenLoansAmount"];
			decimal principalRepaymentAmount = sr["PrincipalRepaymentAmount"];
			bool hasLoans = sr["HasLoans"];

			m_oTrail.MyInputData.Init(DateTime.UtcNow,
				wasManuallyRejected,
				lastManualRejectDate,
				newDataSourceAdded,
				openLoansAmount,
				principalRepaymentAmount,
				hasLoans,
				ConfigManager.CurrentValues.Instance.AutoReRejectMinRepaidPortion,
				ConfigManager.CurrentValues.Instance.AutoReRejectMaxLRDAge
			);

			CheckWasRejected();
			CheckNewMarketPlaceAdded();
			CheckLRDIsTooOld();
			CheckHasNoLoans();

			if (m_oTrail.MyInputData.HasLoans)
				CheckOpenLoansRepayments();

			log.Debug(
				"Primary: checking if auto re-reject should take place for customer {0} complete; {1}", customerId, m_oTrail
			);

			#endregion primary

			#region secondary

			var oSecondary = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(
				customerId, m_oTrail.InputData.DataAsOf, Db, log
			).Init();

			oSecondary.MakeDecision();

			#endregion secondary

			bool bSuccess = m_oTrail.EqualsTo(oSecondary.Trail);

			m_oTrail.Save(Db, oSecondary.Trail);

			if(bSuccess){
				return m_oTrail.HasDecided;
			}

			log.Error("Mismatch in re-rejection logic for customer {0}; main: {1}\n secondory: {2}", customerId, m_oTrail, oSecondary.Trail);
			//not re-rejected if mismatch between two implementations
			return false;
		} // MakeAndVerifyDecision

		public void MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				if (MakeAndVerifyDecision())
				{
					response.DecidedToReject = true;
					response.IsReRejected = true;
					response.AutoRejectReason = "Auto Re-Reject";

					response.CreditResult = "Rejected";
					response.UserStatus = "Rejected";
					response.SystemDecision = "Reject";
					response.DecisionName = "Re-rejection";
				}
			}
			catch (Exception ex)
			{
				StepNoReReject<ExceptionThrown>(true).Init(ex);
				log.Error(ex, "Exception during re-rejection {0}", m_oTrail);
			}
		}

		private void CheckWasRejected()
		{
			if (!m_oTrail.MyInputData.WasManuallyRejected)
			{
				StepNoReReject<WasRejected>(true).Init(m_oTrail.MyInputData.WasManuallyRejected);
			}
			else
			{
				StepNoDecision<WasRejected>().Init(m_oTrail.MyInputData.WasManuallyRejected);
			}
		}

		private void CheckNewMarketPlaceAdded()
		{
			if (m_oTrail.MyInputData.LastManualRejectDate.HasValue && m_oTrail.MyInputData.NewDataSourceAdded)
			{
				StepNoReReject<MarketPlaceWasAdded>(true).Init(m_oTrail.MyInputData.NewDataSourceAdded);
			}
			else
			{
				StepNoDecision<MarketPlaceWasAdded>().Init(m_oTrail.MyInputData.NewDataSourceAdded);
			}
		}

		private void CheckLRDIsTooOld()
		{
			if (m_oTrail.MyInputData.LastManualRejectDate.HasValue && (decimal)(m_oTrail.MyInputData.DataAsOf - m_oTrail.MyInputData.LastManualRejectDate.Value).TotalDays > m_oTrail.MyInputData.AutoReRejectMaxLRDAge)
			{
				StepNoReReject<LRDIsTooOld>(true).Init((decimal)(m_oTrail.MyInputData.DataAsOf - m_oTrail.MyInputData.LastManualRejectDate.Value).TotalDays, m_oTrail.MyInputData.AutoReRejectMaxLRDAge);
			}
			else
			{
				var days = m_oTrail.MyInputData.LastManualRejectDate.HasValue ? (decimal)(m_oTrail.MyInputData.DataAsOf - m_oTrail.MyInputData.LastManualRejectDate.Value).TotalDays : 0.0M;
				StepNoDecision<LRDIsTooOld>().Init(days, m_oTrail.MyInputData.AutoReRejectMaxLRDAge);
			}

		}

		private void CheckHasNoLoans()
		{
			StepNoDecision<TotalLoanCount>().Init(m_oTrail.MyInputData.HasLoans ? 1 : 0);
		}

		private void CheckOpenLoansRepayments() {
			decimal ratio = m_oTrail.MyInputData.OpenLoansAmount == 0 ? 1 : m_oTrail.MyInputData.PrincipalRepaymentAmount / m_oTrail.MyInputData.OpenLoansAmount;

			if (ratio < m_oTrail.MyInputData.AutoReRejectMinRepaidPortion)
			{
				StepReReject<OpenLoansRepayments>(true).Init(
					m_oTrail.MyInputData.OpenLoansAmount,
					m_oTrail.MyInputData.PrincipalRepaymentAmount,
					m_oTrail.MyInputData.AutoReRejectMinRepaidPortion
				);
			}
			else
			{
				StepNoDecision<OpenLoansRepayments>().Init(
					m_oTrail.MyInputData.OpenLoansAmount,
					m_oTrail.MyInputData.PrincipalRepaymentAmount,
					m_oTrail.MyInputData.AutoReRejectMinRepaidPortion
				);
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
