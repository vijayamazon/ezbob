namespace AutomationCalculator.AutoDecision.AutoReApproval {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using Common;
	using Ezbob.Database;
	using ProcessHistory;
	using ProcessHistory.Common;
	using ProcessHistory.ReApproval;
	using ProcessHistory.Trails;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Logger;

	public class Agent {
		#region public

		#region constructor

		public Agent(AConnection oDB, ASafeLog oLog, int customerId, DateTime? dataAsOf = null) {
			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();
			CustomerId = customerId;
			Now = dataAsOf.HasValue? dataAsOf.Value : DateTime.UtcNow;
			Trail = new ReapprovalTrail(customerId, m_oLog);
			Result = new ReApprovalResult(false, 0);
		} // constructor

		#endregion constructor

		public ReApprovalInputData GetInputData() {
			DbHelper dbHelper = new DbHelper(m_oDB, m_oLog);
			AutoReApprovalInputDataModelDb dbData = dbHelper.GetAutoReApprovalInputData(CustomerId);
			AvailableFunds availableFunds = dbHelper.GetAvailableFunds();

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

			m_oLog.Debug("AutoReApprovalInputDataModelDb: {0}, {1}, {2}, {3}", dbData.ApprovedAmount, dbData.TookLoanAmount, dbData.RepaidPrincipal, dbData.SetupFee);
			m_oLog.Debug("ReApprovalInputData = {0}", model.Serialize());

			return model;
		}

		#region method MakeDecision

		public void MakeDecision(ReApprovalInputData data) {
			m_oLog.Debug("Secondary: checking if auto re-approval should take place for customer {0}...", CustomerId);

			try {
				Trail.MyInputData.Init(Now, data);

				CheckInit(data);
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
					StepFailed<Complete>().Init(m_nApprovedAmount);
			}
			catch (Exception e) {
				m_oLog.Error(e, "Exception during auto approval.");
				StepFailed<ExceptionThrown>().Init(e);
			} // try

			if (Trail.HasDecided)
				Result = new ReApprovalResult(true, m_nApprovedAmount);

			m_oLog.Debug(
				"Secondary: checking if auto re-approval should take place for customer {0} complete; {1}\n{2}", CustomerId, Trail,
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
				StepFailed<ApprovedAmount>().Init(m_nApprovedAmount);
		} // SetApprovedAmount

		private void CheckInit(ReApprovalInputData oData) {
			var oErrors = new List<string>();

			if (!oData.ManualApproveDate.HasValue)
				oErrors.Add("last approved cash request time not filled");

			if (oErrors.Count == 0)
				StepDone<InitialAssignment>().Init(oErrors);
			else
				StepFailed<InitialAssignment>().Init(oErrors);
		} // CheckInit

		private void CheckAvailableFunds()
		{
			if (m_nApprovedAmount < Trail.MyInputData.AvaliableFunds)
				StepDone<EnoughFunds>().Init(m_nApprovedAmount, Trail.MyInputData.AvaliableFunds);
			else
				StepFailed<EnoughFunds>().Init(m_nApprovedAmount, Trail.MyInputData.AvaliableFunds);
		}

		private void CheckHasLoanCharges()
		{
			if (!Trail.MyInputData.HasLoanCharges)
				StepDone<Charges>().Init(1); // Not retrieving charges amount???
			else
				StepFailed<Charges>().Init(0);
		}

		private void CheckHasOutstandingLoans()
		{
			if (Trail.MyInputData.NumOutstandingLoans <= Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans)
				StepDone<OutstandingLoanCount>().Init(Trail.MyInputData.NumOutstandingLoans, Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans);
			else
				StepFailed<OutstandingLoanCount>().Init(Trail.MyInputData.NumOutstandingLoans, Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans);
		}

		private void CheckHasAddedMp()
		{
			if (!Trail.MyInputData.NewDataSourceAdded)
				StepDone<NewMarketplace>().Init(Trail.MyInputData.NewDataSourceAdded);
			else
				StepFailed<NewMarketplace>().Init(Trail.MyInputData.NewDataSourceAdded);
		}

		private void CheckHasLatePayment()
		{
			if (Trail.MyInputData.MaxLateDays <=  Trail.MyInputData.AutoReApproveMaxLatePayment)
				StepDone<LatePayment>().Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);
			else
				StepFailed<LatePayment>().Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);
		}

		private void CheckHasLateLoans()
		{
			if (!Trail.MyInputData.WasLate)
				StepDone<LateLoans>().Init();
			else
				StepFailed<LateLoans>().Init();
		}

		private void CheckWasRejected()
		{
			if (!Trail.MyInputData.WasRejected)
				StepDone<RejectAfterLacr>().Init(1, 1);
			else
				StepFailed<RejectAfterLacr>().Init(0, 0);
		}

		private void CheckIsLACRTooOld() {
			int approvedDaysAgo = Trail.MyInputData.ManualApproveDate.HasValue
				? (int)(Now - Trail.MyInputData.ManualApproveDate.Value).TotalDays
				: 0;

			if (Trail.MyInputData.ManualApproveDate.HasValue && approvedDaysAgo <= Trail.MyInputData.AutoReApproveMaxLacrAge)
				StepDone<LacrTooOld>().Init(approvedDaysAgo, Trail.MyInputData.AutoReApproveMaxLacrAge);
			else
				StepFailed<LacrTooOld>().Init(approvedDaysAgo, Trail.MyInputData.AutoReApproveMaxLacrAge);
		}

		private void CheckIsFraud() {
			if (Trail.MyInputData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
		} // CheckIsFraud

		#endregion steps

		/// <summary>
		/// If the step was the only step then overall decision would be 'approved'.
		/// </summary>
		/// <typeparam name="T">Step type.</typeparam>
		/// <returns>Step type instance for filling step details.</returns>
		private T StepDone<T>() where T : ATrace
		{
			return Trail.Affirmative<T>(false);
		} 

		/// <summary>
		/// Sets overall decision to 'no approve'.
		/// </summary>
		/// <typeparam name="T">Step type.</typeparam>
		/// <returns>Step type instance for filling step details.</returns>
		private T StepFailed<T>() where T : ATrace {
			m_nApprovedAmount = 0;
			return Trail.Negative<T>(true);
		} 

		#region fields

		protected readonly DateTime Now;
		protected readonly int CustomerId;

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;
		private int m_nApprovedAmount;

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
