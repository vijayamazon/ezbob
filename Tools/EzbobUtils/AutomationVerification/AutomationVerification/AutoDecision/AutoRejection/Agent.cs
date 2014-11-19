namespace AutomationCalculator.AutoDecision.AutoRejection
{
	using System;
	using AutomationCalculator.ProcessHistory.Trails;
	using ProcessHistory;
	using ProcessHistory.Common;
	using Ezbob.Database;
	using Ezbob.Logger;
	using ProcessHistory.Trails;

	public class RejectionAgent
	{
		#region public

		#region constructor

		public RejectionAgent(int nCustomerID, AConnection oDB, ASafeLog oLog)
		{
			_customerId = nCustomerID;
			_isAutoRejected = false;

			Trail = new RejectionTrail(nCustomerID, oLog);
		} // constructor

		#endregion constructor

		#region method MakeDecision

		public void MakeDecision()
		{
			m_oLog.Debug("Checking if auto reject should take place for customer {0}...", _customerId);
			try {
				CheckRejectionExceptions();
				Trail.LockDecision();
				CheckRejections();
			}
			catch (Exception e)
			{
				m_oLog.Error(e, "Exception during auto rejection.");
				StepNoDecision<ExceptionThrown>().Init(e);
			} // try

			m_oLog.Debug(
				"Checking if auto rejection should take place for customer {0} complete; {1}\n{2}",
				_customerId,
				Trail,
				_isAutoRejected ? "Auto rejected" : "Not auto rejected"
			);
		}

		private void CheckRejections() {
			
		}

		private void CheckRejectionExceptions() {
			throw new NotImplementedException();
		}

// MakeDecision

		#endregion method MakeDecision

		public RejectionTrail Trail { get; private set; }

		#endregion public

		#region private

		#region steps


		#endregion steps

		#region method ProcessRow

		private void ProcessRow(SafeReader sr)
		{

		} // ProcessRow

		#endregion method ProcessRow
		
		private T StepReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace
		{
			return Trail.Affirmative<T>(bLockDecisionAfterAddingAStep);
		} // StepReject

		private T StepNoReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace
		{
			return Trail.Negative<T>(bLockDecisionAfterAddingAStep);
		} // StepNoReject

		private T StepNoDecision<T>() where T : ATrace
		{
			return Trail.Dunno<T>();
		} // StepReject


		#region fields

		private readonly DateTime Now;
		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private int _customerId;
		private bool _isAutoRejected;

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
