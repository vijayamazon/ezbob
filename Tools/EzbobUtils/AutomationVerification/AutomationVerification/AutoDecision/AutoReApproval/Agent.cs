namespace AutomationCalculator.AutoDecision.AutoReApproval {
	using System;
	using Common;
	using ProcessHistory;
	using ProcessHistory.Common;
	using ProcessHistory.ReApproval;
	using ProcessHistory.Trails;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Logger;

	public class Agent {
		#region public

		#region constructor

		public Agent(ASafeLog oLog, int customerId, DateTime? dataAsOf = null) {
			m_oLog = oLog ?? new SafeLog();
			CustomerId = customerId;
			Now = dataAsOf.HasValue? dataAsOf.Value : DateTime.UtcNow;
			Trail = new ReapprovalTrail(customerId, m_oLog);
			Result = new ReApprovalResult(false, 0);
		} // constructor

		#endregion constructor

		public ReApprovalInputData GetInputData() {
			var dbHelper = new DbHelper(m_oLog);
			var dbData = dbHelper.GetAutoReApprovalInputData(CustomerId);
			var availableFunds = dbHelper.GetAvailableFunds();
			var model = new ReApprovalInputData {
				AutoReApproveMaxLacrAge = dbData.AutoReApproveMaxLacrAge,
				AutoReApproveMaxLatePayment = dbData.AutoReApproveMaxLatePayment,
				AutoReApproveMaxNumOfOutstandingLoans = dbData.AutoReApproveMaxNumOfOutstandingLoans,
				AvaliableFunds = availableFunds.Available - availableFunds.Reserved,
				HasLoanCharges = dbData.HasLoanCharges,
				FraudStatus = (FraudStatus)dbData.FraudStatus,
				ManualApproveDate = dbData.ManualApproveDate,
				MaxLateDays = dbData.MaxLateDays,
				NewDataSourceAdded = dbData.NewDataSourceAdded,
				NumOutstandingLoans = dbData.NumOutstandingLoans,
				ReApproveAmount = dbData.ApprovedAmount - dbData.TookLoanAmount + dbData.RepaidPrincipal + dbData.SetupFee,
				WasLate = dbData.WasLate,
				WasRejected = dbData.WasRejected,
			};

			return model;
		}

		#region method MakeDecision

		public void MakeDecision(ReApprovalInputData data) {
			m_oLog.Debug("Checking if auto re approval should take place for customer {0}...", CustomerId);

			try {
				Trail.MyInputData.Init(Now, data);

				CheckInit();
				CheckIsFraud();
				CheckIsLACRTooOld();
				CheckWasRejected();
				CheckHasLateLoans();
				CheckHasLatePayment();
				CheckHasAddedMp();
				CheckHasOutstandingLoans();
				CheckHasLoanCharges();
				SetApprovedAmount((int)data.ReApproveAmount);
				CheckAvailableFunds();

				if (m_nApprovedAmount > 0)
					StepDone<Complete>().Init(m_nApprovedAmount);
				else
					StepNoReApprove<Complete>().Init(m_nApprovedAmount);
			}
			catch (Exception e) {
				m_oLog.Error(e, "Exception during auto approval.");
				StepNoReApprove<ExceptionThrown>().Init(e);
			} // try

			if (Trail.HasDecided)
				Result = new ReApprovalResult(true, m_nApprovedAmount);

			m_oLog.Debug(
				"Checking if auto re approval should take place for customer {0} complete; {1}\n{2}", CustomerId, Trail,
				Result == null ? string.Empty : Result + "."
			);
		} // MakeDecision

		#endregion method MakeDecision

		public ReApprovalResult Result { get; private set; }

		public ReapprovalTrail Trail { get; private set; }

		#endregion public

		#region private

		#region steps

		private void SetApprovedAmount(int nApprovedAmount) {
			if (Trail.HasDecided)
				m_nApprovedAmount = nApprovedAmount;

			if (m_nApprovedAmount > 0)
				StepDone<ApprovedAmount>().Init(m_nApprovedAmount);
			else
				StepNoReApprove<ApprovedAmount>().Init(m_nApprovedAmount);
		} // SetApprovedAmount

		private void CheckInit() {
			StepDone<InitialAssignment>().Init(null);
		} // CheckInit

		private void CheckAvailableFunds()
		{
			if (Trail.MyInputData.AvaliableFunds <= Trail.MyInputData.ReApproveAmount)
				StepDone<EnoughFunds>().Init(Trail.MyInputData.ReApproveAmount, Trail.MyInputData.AvaliableFunds);
			else
				StepNoReApprove<EnoughFunds>().Init(Trail.MyInputData.ReApproveAmount, Trail.MyInputData.AvaliableFunds);
		}

		private void CheckHasLoanCharges()
		{
			if (!Trail.MyInputData.HasLoanCharges)
				StepDone<Charges>().Init(1); // Not retrieving charges amount???
			else
				StepNoReApprove<Charges>().Init(0);
		}

		private void CheckHasOutstandingLoans()
		{
			if (Trail.MyInputData.NumOutstandingLoans <= Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans)
				StepDone<OutstandingLoanCount>().Init(Trail.MyInputData.NumOutstandingLoans, Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans);
			else
				StepNoReApprove<OutstandingLoanCount>().Init(Trail.MyInputData.NumOutstandingLoans, Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans);
		}

		private void CheckHasAddedMp()
		{
			if (!Trail.MyInputData.NewDataSourceAdded)
				StepDone<NewMarketplace>().Init(Trail.MyInputData.NewDataSourceAdded);
			else
				StepNoReApprove<NewMarketplace>().Init(Trail.MyInputData.NewDataSourceAdded);
		}

		private void CheckHasLatePayment()
		{
			if (Trail.MyInputData.MaxLateDays <=  Trail.MyInputData.AutoReApproveMaxLatePayment)
				StepDone<LatePayment>().Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);
			else
				StepNoReApprove<LatePayment>().Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);
		}

		private void CheckHasLateLoans()
		{
			if (!Trail.MyInputData.WasLate)
				StepDone<LateLoans>().Init();
			else
				StepNoReApprove<LateLoans>().Init();
		}

		private void CheckWasRejected()
		{
			if (!Trail.MyInputData.WasRejected)
				StepDone<RejectAfterLacr>().Init(1, 1);
			else
				StepNoReApprove<RejectAfterLacr>().Init(0, 0);
		}

		private void CheckIsLACRTooOld() {
			var approvedDaysAgo = Trail.MyInputData.ManualApproveDate.HasValue ? (decimal)(Now - Trail.MyInputData.ManualApproveDate.Value).TotalDays : 0.0M;
			if (Trail.MyInputData.ManualApproveDate.HasValue && approvedDaysAgo <= Trail.MyInputData.AutoReApproveMaxLacrAge)
				StepDone<LacrTooOld>().Init(approvedDaysAgo, Trail.MyInputData.AutoReApproveMaxLacrAge);
			else
				StepNoReApprove<LacrTooOld>().Init(approvedDaysAgo, Trail.MyInputData.AutoReApproveMaxLacrAge);
		}

		private void CheckIsFraud() {
			if (Trail.MyInputData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
			else
				StepNoReApprove<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
		} // CheckIsFraud

		#endregion steps

		private T StepDone<T>() where T : ATrace
		{
			m_nApprovedAmount = 0;
			return Trail.Affirmative<T>(false);
		} 

		private T StepNoReApprove<T>() where T : ATrace {
			m_nApprovedAmount = 0;
			return Trail.Negative<T>(false);
		} 

		//private T StepReApprove<T>() where T : ATrace {
		//	return Trail.Affirmative<T>(false);
		//} 

		#region fields

		protected readonly DateTime Now;
		protected readonly int CustomerId;

		private readonly ASafeLog m_oLog;
		private int m_nApprovedAmount;

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
