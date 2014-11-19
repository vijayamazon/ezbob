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
				StepNoReReject<ExceptionThrown>().Init(ex);
				log.Error("Exception during rerejection:{0}", ex);
				return false;
			}
		}

		private void CheckWasRejected()
		{
			if (!m_oTrail.MyInputData.WasManuallyRejected)
			{
				StepNoReReject<WasRejected>().Init(m_oTrail.MyInputData.WasManuallyRejected);
				m_oTrail.LockDecision();
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
				StepNoReReject<MarketPlaceWasAdded>().Init(m_oTrail.MyInputData.NewDataSourceAdded);
				m_oTrail.LockDecision();
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
				StepNoReReject<LRDIsTooOld>().Init((decimal)(m_oTrail.MyInputData.DataAsOf - m_oTrail.MyInputData.LastManualRejectDate.Value).TotalDays, m_oTrail.MyInputData.AutoReRejectMaxLRDAge);
				m_oTrail.LockDecision();
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
				StepReReject<HasNoLoans>().Init(m_oTrail.MyInputData.HasLoans);
				m_oTrail.LockDecision();
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
				StepReReject<OpenLoansRepayments>().Init(m_oTrail.MyInputData.OpenLoansAmount, m_oTrail.MyInputData.PrincipalRepaymentAmount,m_oTrail.MyInputData.AutoReRejectMinRepaidPortion);
				m_oTrail.LockDecision();
			}
			else
			{
				StepNoDecision<OpenLoansRepayments>().Init(m_oTrail.MyInputData.OpenLoansAmount, m_oTrail.MyInputData.PrincipalRepaymentAmount, m_oTrail.MyInputData.AutoReRejectMinRepaidPortion);
			}
		}

		private T StepNoReReject<T>() where T : ATrace
		{

			return m_oTrail.Negative<T>();
		}

		private T StepReReject<T>() where T : ATrace
		{
			return m_oTrail.Affirmative<T>();
		}

		private T StepNoDecision<T>() where T : ATrace
		{
			return m_oTrail.Dunno<T>();
		}
	}
}
