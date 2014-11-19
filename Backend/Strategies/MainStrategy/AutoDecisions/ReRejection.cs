namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions
{
	using System;
	using AutomationCalculator.AutoDecision.AutoReRejection;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoReRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ReRejection
	{
		private readonly AConnection Db;
		private readonly ASafeLog log;
		private readonly int customerId;
		private ReRejectionTrail m_oTrail;
		public ReRejection(int customerId, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.customerId = customerId;
			m_oTrail = new ReRejectionTrail(customerId, oLog);
		}

		public bool MakeDecision(AutoDecisionRejectionResponse response)
		{
			try
			{
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
					ConfigManager.CurrentValues.Instance.AutoReRejectMaxLRDAge);

				CheckWasRejected();
				CheckNewMarketPlaceAdded();
				CheckLRDIsTooOld();
				CheckHasNoLoans();
				CheckOpenLoansRepayments();

				log.Debug("{0}", m_oTrail);
				if (m_oTrail.DecisionStatus == DecisionStatus.Affirmative)
				{
					response.IsReRejected = true;
					response.AutoRejectReason = "Auto Re-Reject";

					response.CreditResult = "Rejected";

					response.UserStatus = "Rejected";
					response.SystemDecision = "Reject";
					response.DecidedToReject = true;
					response.DecisionName = "Re-rejection";

					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				StepNoReReject<ExceptionThrown>(true).Init(ex);
				log.Error("Exception during rerejection {0}", m_oTrail);
				//log.Error("Exception during rerejection:{0}", ex);
				return false;
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
				StepNoReReject<LRDIsTooOld>(false).Init((decimal)(m_oTrail.MyInputData.DataAsOf - m_oTrail.MyInputData.LastManualRejectDate.Value).TotalDays, m_oTrail.MyInputData.AutoReRejectMaxLRDAge);
			}
			else
			{
				var days = m_oTrail.MyInputData.LastManualRejectDate.HasValue ? (decimal)(m_oTrail.MyInputData.DataAsOf - m_oTrail.MyInputData.LastManualRejectDate.Value).TotalDays : 0.0M;
				StepNoDecision<LRDIsTooOld>().Init(days, m_oTrail.MyInputData.AutoReRejectMaxLRDAge);
			}

		}

		private void CheckHasNoLoans()
		{
			if (!m_oTrail.MyInputData.HasLoans)
			{
				StepReReject<HasNoLoans>(false).Init(m_oTrail.MyInputData.HasLoans);
			}
			else
			{
				StepNoDecision<HasNoLoans>().Init(m_oTrail.MyInputData.HasLoans);
			}
		}

		private void CheckOpenLoansRepayments()
		{
			if (m_oTrail.MyInputData.PrincipalRepaymentAmount * m_oTrail.MyInputData.AutoReRejectMinRepaidPortion < m_oTrail.MyInputData.OpenLoansAmount)
			{
				StepReReject<OpenLoansRepayments>(false).Init(m_oTrail.MyInputData.OpenLoansAmount, m_oTrail.MyInputData.PrincipalRepaymentAmount,m_oTrail.MyInputData.AutoReRejectMinRepaidPortion);
			}
			else
			{
				StepNoDecision<OpenLoansRepayments>().Init(m_oTrail.MyInputData.OpenLoansAmount, m_oTrail.MyInputData.PrincipalRepaymentAmount, m_oTrail.MyInputData.AutoReRejectMinRepaidPortion);
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
