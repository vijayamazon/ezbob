namespace AutomationCalculator.AutoDecision.AutoRejection
{
	using System;
	using ProcessHistory;
	using ProcessHistory.Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class RejectionAgent
	{
		#region public

		#region constructor

		public RejectionAgent(int nCustomerID, AConnection oDB, ASafeLog oLog)
		{
			_customerId = nCustomerID;
			_isAutoRejected = false;
		} // constructor

		#endregion constructor

		#region method MakeDecision

		// public void MakeDecision(AutoDecisionResponse response) {
		public void MakeDecision()
		{
			m_oLog.Debug("Checking if auto reject should take place for customer {0}...", _customerId);


			try
			{

			}
			catch (Exception e)
			{
				m_oLog.Error(e, "Exception during auto rejection.");
				StepFailed<ExceptionThrown>().Init(e);
			} // try

			m_oLog.Debug(
				"Checking if auto rejection should take place for customer {0} complete; {1}\n{2}",
				_customerId,
				Trail,
				_isAutoRejected ? "Auto rejected" : "Not auto rejected"
			);
		} // MakeDecision

		#endregion method MakeDecision

		public Trail Trail { get; private set; }

		#endregion public

		#region private

		#region steps


		#endregion steps

		#region method ProcessRow

		private void ProcessRow(SafeReader sr)
		{

		} // ProcessRow

		#endregion method ProcessRow


		#region method StepFailed

		private T StepFailed<T>() where T : ATrace
		{
			return Trail.Failed<T>();
		} // StepFailed

		#endregion method StepFailed

		#region method StepDone

		private T StepDone<T>() where T : ATrace
		{
			return Trail.Done<T>();
		} // StepFailed

		#endregion method StepDone

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
