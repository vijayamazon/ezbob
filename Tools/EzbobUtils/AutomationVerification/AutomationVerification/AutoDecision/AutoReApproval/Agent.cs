namespace AutomationCalculator.AutoDecision.AutoReApproval {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.ReApproval;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class Agent {
		public ReApprovalResult Result { get; private set; }

		public ReapprovalTrail Trail { get; private set; }

		public Agent(
			AConnection oDB,
			ASafeLog oLog,
			int customerId,
			long? cashRequestID,
			long? nlCashRequestID,
			DateTime? dataAsOf = null
		) {
			this.m_oDB = oDB;
			this.m_oLog = oLog.Safe();
			this.CustomerId = customerId;
			this.Now = dataAsOf ?? DateTime.UtcNow;
			Trail = new ReapprovalTrail(customerId, cashRequestID, nlCashRequestID, this.m_oLog);
			Result = new ReApprovalResult(false, 0);
		} // constructor

		public ReApprovalInputData GetInputData() {
			DbHelper dbHelper = new DbHelper(this.m_oDB, this.m_oLog);
			AutoReApprovalInputDataModelDb dbData = dbHelper.GetAutoReApprovalInputData(this.CustomerId);
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
				ReApproveAmount = dbData.ApprovedAmount - dbData.TookLoanAmount + dbData.RepaidPrincipal,
				WasLate = dbData.WasLate,
				WasRejected = dbData.WasRejected,
				MinLoan = dbData.MinLoan,
				LacrID = dbData.LacrID,
			};

			this.m_oLog.Debug(
				"AutoReApprovalInputDataModelDb: {0}, {1}, {2}, {3}",
				dbData.ApprovedAmount,
				dbData.TookLoanAmount,
				dbData.RepaidPrincipal,
				dbData.SetupFee
			);
			this.m_oLog.Debug("ReApprovalInputData = {0}", model.Serialize());

			return model;
		} // GetInputData

		public void MakeDecision(ReApprovalInputData data) {
			this.m_oLog.Debug(
				"Secondary: checking if auto re-approval should take place for customer {0}...",
				this.CustomerId
			);

			try {
				Trail.MyInputData.Init(this.Now, data);

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

				if (this.m_nApprovedAmount >= Trail.MyInputData.MinLoan)
					StepDone<Complete>().Init(this.m_nApprovedAmount, Trail.MyInputData.MinLoan, units: "£");
				else
					StepFailed<Complete>().Init(this.m_nApprovedAmount, Trail.MyInputData.MinLoan, units: "£");
			} catch (Exception e) {
				this.m_oLog.Error(e, "Exception during auto approval.");
				StepFailed<ExceptionThrown>().Init(e);
			} // try

			if (Trail.HasDecided)
				Result = new ReApprovalResult(true, this.m_nApprovedAmount);

			this.m_oLog.Debug(
				"Secondary: checking if auto re-approval should take place for customer {0} complete; {1}\n{2}",
				this.CustomerId,
				Trail,
				Result == null ? string.Empty : Result + "."
			);
		} // MakeDecision

		protected readonly DateTime Now;
		protected readonly int CustomerId;

		private void SetApprovedAmount(int nApprovedAmount) {
			if (Trail.HasDecided)
				this.m_nApprovedAmount = nApprovedAmount;

			if (this.m_nApprovedAmount > 0)
				StepDone<ApprovedAmount>().Init(this.m_nApprovedAmount);
			else
				StepFailed<ApprovedAmount>().Init(this.m_nApprovedAmount);
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

		private void CheckAvailableFunds() {
			if (this.m_nApprovedAmount < Trail.MyInputData.AvaliableFunds)
				StepDone<EnoughFunds>().Init(this.m_nApprovedAmount, Trail.MyInputData.AvaliableFunds);
			else
				StepFailed<EnoughFunds>().Init(this.m_nApprovedAmount, Trail.MyInputData.AvaliableFunds);
		} // CheckAvailableFunds

		private void CheckHasLoanCharges() {
			if (!Trail.MyInputData.HasLoanCharges)
				StepDone<Charges>().Init(1); // Not retrieving charges amount???
			else
				StepFailed<Charges>().Init(0);
		} // CheckHasLoanCharges

		private void CheckHasOutstandingLoans() {
			if (Trail.MyInputData.NumOutstandingLoans <= Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans) {
				StepDone<OutstandingLoanCount>()
					.Init(Trail.MyInputData.NumOutstandingLoans, Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans);
			} else {
				StepFailed<OutstandingLoanCount>()
					.Init(Trail.MyInputData.NumOutstandingLoans, Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans);
			} // if
		} // CheckHasOutstandingLoans

		private void CheckHasAddedMp() {
			if (!Trail.MyInputData.NewDataSourceAdded)
				StepDone<NewMarketplace>().Init(Trail.MyInputData.NewDataSourceAdded);
			else
				StepFailed<NewMarketplace>().Init(Trail.MyInputData.NewDataSourceAdded);
		} // CheckHasAddedMp

		private void CheckHasLatePayment() {
			if (Trail.MyInputData.MaxLateDays <= Trail.MyInputData.AutoReApproveMaxLatePayment)
				StepDone<LatePayment>().Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);
			else
				StepFailed<LatePayment>().Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);
		} // CheckHasLatePayment

		private void CheckHasLateLoans() {
			if (!Trail.MyInputData.WasLate)
				StepDone<LateLoans>().Init(Trail.MyInputData.WasLate);
			else
				StepFailed<LateLoans>().Init(Trail.MyInputData.WasLate);
		} // CheckHasLateLoans

		private void CheckWasRejected() {
			if (!Trail.MyInputData.WasRejected)
				StepDone<RejectAfterLacr>().Init(0, 0);
			else
				StepFailed<RejectAfterLacr>().Init(1, 1);
		} // CheckWasRejected

		private void CheckIsLACRTooOld() {
			int approvedDaysAgo = Trail.MyInputData.ManualApproveDate.HasValue
				? (int)(this.Now - Trail.MyInputData.ManualApproveDate.Value).TotalDays
				: 0;

			if (Trail.MyInputData.ManualApproveDate.HasValue && approvedDaysAgo <= Trail.MyInputData.AutoReApproveMaxLacrAge)
				StepDone<LacrTooOld>().Init(approvedDaysAgo, Trail.MyInputData.AutoReApproveMaxLacrAge);
			else
				StepFailed<LacrTooOld>().Init(approvedDaysAgo, Trail.MyInputData.AutoReApproveMaxLacrAge);
		} // CheckIsLACRTooOld

		private void CheckIsFraud() {
			if (Trail.MyInputData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
		} // CheckIsFraud

		/// <summary>
		/// If the step was the only step then overall decision would be 'approved'.
		/// </summary>
		/// <typeparam name="T">Step type.</typeparam>
		/// <returns>Step type instance for filling step details.</returns>
		private T StepDone<T>() where T : ATrace {
			return Trail.Affirmative<T>(false);
		} // StepDone

		/// <summary>
		///     Sets overall decision to 'no approve'.
		/// </summary>
		/// <typeparam name="T">Step type.</typeparam>
		/// <returns>Step type instance for filling step details.</returns>
		private T StepFailed<T>() where T : ATrace {
			this.m_nApprovedAmount = 0;
			return Trail.Negative<T>(true);
		} // StepFailed

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;
		private int m_nApprovedAmount;
	} // class Agent
} // namespace
