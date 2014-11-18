﻿namespace AutomationCalculator.AutoDecision.AutoReApproval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutoApproval;
	using Common;
	using ProcessHistory;
	using ProcessHistory.Common;
	using ProcessHistory.AutoApproval;
	using ProcessHistory.ReApproval;
	using ProcessHistory.Trails;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
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

				// Once a step is not passed there is no need to continue result-wise. However the
				// process continues because we want to pick all the possible reasons for not
				// approving a customer in order to compare different implementations of the process.

				//if ((m_nApprovedAmount > 0))
				//	StepDone<InitialAssignment>().Init(m_nApprovedAmount);
				//else
				//	StepFailed<InitialAssignment>().Init(m_nApprovedAmount);

				CheckIsFraud();
				CheckIsLACRTooOld();
				CheckWasRejected();
				CheckHasLateLoans();
				CheckHasLatePayment();
				CheckHasAddedMp();
				CheckHasOutstandingLoans();
				CheckHasLoanCharges();
				CheckAvailableFunds();

				m_nApprovedAmount = (int)data.ReApproveAmount;
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
				"Checking if auto approval should take place for customer {0} complete; {1}\n{2}", CustomerId, Trail,
				Result == null ? string.Empty : "ReApproved " + Result + "."
			);
		}

// MakeDecision

		#endregion method MakeDecision

		public ReApprovalResult Result { get; private set; }

		public ReapprovalTrail Trail { get; private set; }

		#endregion public

		#region private

		#region steps

		private void CheckAvailableFunds()
		{
			if (Trail.MyInputData.AvaliableFunds <= Trail.MyInputData.ReApproveAmount)
				StepDone<EnoughFunds>().Init(Trail.MyInputData.ReApproveAmount, Trail.MyInputData.AvaliableFunds);
			else
				StepFailed<EnoughFunds>().Init(Trail.MyInputData.ReApproveAmount, Trail.MyInputData.AvaliableFunds);
		}

		private void CheckHasLoanCharges()
		{
			if (!Trail.MyInputData.HasLoanCharges)
				StepDone<Charges>().Init(1); // Why to retrieve charges amount???
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
			if (Trail.MyInputData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
		}

		private void CheckHasLatePayment()
		{
			if (Trail.MyInputData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
		}

		private void CheckHasLateLoans()
		{
			if (Trail.MyInputData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
		}

		private void CheckWasRejected()
		{
			if (Trail.MyInputData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
		}

		private void CheckIsLACRTooOld()
		{
			if (Trail.MyInputData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
		}

		private void CheckIsFraud() {
			if (Trail.MyInputData.FraudStatus == FraudStatus.Ok)
				StepDone<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
			else
				StepFailed<FraudSuspect>().Init(Trail.MyInputData.FraudStatus);
		} // CheckIsFraud

		#endregion steps
		
		#region method StepFailed

		private T StepFailed<T>() where T : ATrace {
			m_nApprovedAmount = 0;
			return Trail.Dunno<T>();
		} // StepFailed

		#endregion method StepFailed

		#region method StepDone

		private T StepDone<T>() where T : ATrace {
			return Trail.Affirmative<T>();
		} // StepFailed

		#endregion method StepDone

		#region fields

		protected readonly DateTime Now;
		protected readonly int CustomerId;

		private readonly ASafeLog m_oLog;
		private int m_nApprovedAmount;
		

		#endregion fields

		#endregion private
	} // class Agent
} // namespace
