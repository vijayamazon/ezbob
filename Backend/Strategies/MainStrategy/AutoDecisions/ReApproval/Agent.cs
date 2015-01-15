﻿namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.ReApproval;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Backend.Strategies.Misc;
	using EZBob.DatabaseLib.Model.Database;

	public class Agent {
		public virtual ReapprovalTrail Trail { get; private set; }

		public virtual decimal ApprovedAmount { get; private set; }

		public Agent(int nCustomerID, AConnection oDB, ASafeLog oLog) {
			DB = oDB;
			Log = oLog ?? new SafeLog();
			Args = new Arguments(nCustomerID);
		} // constructor

		public virtual Agent Init() {
			MetaData = new MetaData();
			LatePayments = new List<Payment>();
			NewMarketplaces = new List<Marketplace>();
			ApprovedAmount = 0;

			Trail = new ReapprovalTrail(Args.CustomerID, Log, CurrentValues.Instance.AutomationExplanationMailReciever, CurrentValues.Instance.MailSenderEmail, CurrentValues.Instance.MailSenderName);
			Cfg = new Configuration(DB, Log);

			return this;
		} // Init

		public virtual bool MakeAndVerifyDecision() {
			RunPrimary();

			AutomationCalculator.AutoDecision.AutoReApproval.Agent oSecondary = RunSecondary();

			bool bSuccess = Trail.EqualsTo(oSecondary.Trail);

			Log.Debug("Status check result: {0}.", bSuccess ? "success" : "fail");

			if (bSuccess && Trail.HasDecided) {
				Log.Debug("Match and approved.");

				if (ApprovedAmount == oSecondary.Result.ReApproveAmount) {
					Log.Debug("Match and approved and same amount of {0}.", ApprovedAmount);

					Trail.Affirmative<SameAmount>(false)
						.Init(ApprovedAmount);
					oSecondary.Trail.Affirmative<SameAmount>(false)
						.Init(oSecondary.Result.ReApproveAmount);
				} else {
					Log.Debug("Match and approved but different amount: {0} vs {1}.", ApprovedAmount, oSecondary.Result.ReApproveAmount);

					Trail.Negative<SameAmount>(false)
						.Init(ApprovedAmount);
					oSecondary.Trail.Negative<SameAmount>(false)
						.Init(oSecondary.Result.ReApproveAmount);
					bSuccess = false;
				} // if
			} // if

			Trail.Save(DB, oSecondary.Trail);

			return bSuccess;
		} // MakeAndVerifyDecision

		public virtual void MakeDecision(AutoDecisionResponse response) {
			try {
				if (MakeAndVerifyDecision() && Trail.HasDecided) {
					response.AutoApproveAmount = (int)ApprovedAmount;
					response.Decision = DecisionActions.ReApprove;
					response.CreditResult = CreditResultStatus.Approved;
					response.UserStatus = Status.Approved;
					response.SystemDecision = SystemDecision.Approve;
					response.LoanOfferUnderwriterComment = "Auto Re-Approval";
					response.DecisionName = "Re-Approval";
					response.AppValidFor = DateTime.UtcNow.AddDays(MetaData.OfferLength);
					response.LoanOfferEmailSendingBannedNew = MetaData.IsEmailSendingBanned;

					var sr = DB.GetFirst("RapprovalGetLastApproveTerms", CommandSpecies.StoredProcedure, new QueryParameter("LacrID", MetaData.LacrID));
					response.InterestRate = sr["InterestRate"];
					response.SetupFeeEnabled = sr["UseSetupFee"];
					response.RepaymentPeriod = sr["RepaymentPeriod"];
					response.BrokerSetupFeeEnabled = sr["UseBrokerSetupFee"];
					response.ManualSetupFeeAmount = sr["ManualSetupFeeAmount"];
					response.ManualSetupFeePercent = sr["ManualSetupFeePercent"];
					response.LoanTypeID = sr["LoanTypeID"];
					response.LoanSourceID = sr["LoanSourceID"];
					response.IsCustomerRepaymentPeriodSelectionAllowed = sr["IsCustomerRepaymentPeriodSelectionAllowed"];
				} // if
			} catch (Exception e) {
				Log.Error(e, "Exception during re-approval.");
				StepFailed<ExceptionThrown>()
					.Init(e);
			} // try

			Log.Msg("Auto re-approved amount: {0}.", ApprovedAmount);
		} // MakeDecision
		protected virtual AConnection DB { get; private set; }

		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }

		protected virtual Arguments Args { get; private set; }

		protected virtual MetaData MetaData { get; private set; }

		protected virtual List<Payment> LatePayments { get; private set; }

		protected virtual List<Marketplace> NewMarketplaces { get; private set; }

		protected virtual void GatherData() {
			Cfg.Load();

			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoReapprovalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID)
				);

			var availFunds = new GetAvailableFunds(DB, Log);
			availFunds.Execute();

			MetaData.Validate();

			Trail.MyInputData.Init(DateTime.UtcNow, null);

			Trail.MyInputData.FraudStatus = MetaData.FraudStatus;
			Trail.MyInputData.ManualApproveDate = MetaData.LacrTime;
			Trail.MyInputData.WasLate = MetaData.LateLoanCount > 0;
			Trail.MyInputData.WasRejected = MetaData.RejectAfterLacrID > 0;
			Trail.MyInputData.MaxLateDays = LatePayments.Count < 1 ? 0 : LatePayments.Select(lp => lp.Delay)
				.Max();
			Trail.MyInputData.NewDataSourceAdded = NewMarketplaces.Count > 0;
			Trail.MyInputData.NumOutstandingLoans = MetaData.OpenLoanCount;
			Trail.MyInputData.HasLoanCharges = MetaData.SumOfCharges > 0.00000001m;
			Trail.MyInputData.ReApproveAmount = MetaData.ApprovedAmount;
			Trail.MyInputData.AvaliableFunds = availFunds.AvailableFunds - availFunds.ReservedAmount;
			Trail.MyInputData.AutoReApproveMaxLacrAge = Cfg.MaxLacrAge;
			Trail.MyInputData.AutoReApproveMaxLatePayment = Cfg.MaxLatePayment;
			Trail.MyInputData.AutoReApproveMaxNumOfOutstandingLoans = Cfg.MaxNumOfOutstandingLoans;
			Trail.MyInputData.MinLoan = ConfigManager.CurrentValues.Instance.MinLoan;
		} // GatherData

		private enum RowType {
			MetaData,
			LatePayment,
			Marketplace,
		} // enum RowType

		private void RunPrimary() {
			Log.Debug("Primary: checking if auto re-approve should take place for customer {0}...", Args.CustomerID);

			GatherData();

			CheckInit();
			CheckIsFraud();
			CheckLacrTooOld();
			CheckRejectAfterLacr();
			CheckLateLoans();
			CheckLatePayments();
			CheckNewMarketplaces();
			CheckOutstandingLoans();
			CheckLoanCharges();
			SetApprovedAmount();
			CheckAvailableFunds();
			CheckComplete();

			Log.Debug("Primary: checking if auto re-approve should take place for customer {0} complete.\n{1}", Args.CustomerID, Trail);
		} // RunPrimary

		private AutomationCalculator.AutoDecision.AutoReApproval.Agent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoReApproval.Agent(DB, Log, Args.CustomerID, Trail.InputData.DataAsOf);
			oSecondary.MakeDecision(oSecondary.GetInputData());

			return oSecondary;
		} // RunSecondary

		private void CheckComplete() {
			if (ApprovedAmount > Trail.MyInputData.MinLoan) {
				StepDone<Complete>()
					.Init(ApprovedAmount, Trail.MyInputData.MinLoan);
			} else {
				StepFailed<Complete>()
					.Init(ApprovedAmount, Trail.MyInputData.MinLoan);
			}
		} // CheckComplete

		private void CheckInit() {
			if (MetaData.ValidationErrors.Count == 0) {
				StepDone<InitialAssignment>()
					.Init(MetaData.ValidationErrors);
			} else {
				StepFailed<InitialAssignment>()
					.Init(MetaData.ValidationErrors);
			}
		} // CheckInit

		private void CheckIsFraud() {
			if (MetaData.FraudStatus == FraudStatus.Ok) {
				StepDone<FraudSuspect>()
					.Init(MetaData.FraudStatus);
			} else {
				StepFailed<FraudSuspect>()
					.Init(MetaData.FraudStatus);
			}
		} // CheckIsFraud

		private void CheckLacrTooOld() {
			if (MetaData.LacrIsTooOld(Cfg.MaxLacrAge)) {
				StepFailed<LacrTooOld>()
					.Init(MetaData.LacrAge ?? -1, Cfg.MaxLacrAge);
			} else {
				StepDone<LacrTooOld>()
					.Init(MetaData.LacrAge ?? -1, Cfg.MaxLacrAge);
			}
		} // CheckLacrTooOld

		private void CheckRejectAfterLacr() {
			if (MetaData.RejectAfterLacrID > 0) {
				StepFailed<RejectAfterLacr>()
					.Init(MetaData.RejectAfterLacrID, MetaData.LacrID);
			} else {
				StepDone<RejectAfterLacr>()
					.Init(MetaData.RejectAfterLacrID, MetaData.LacrID);
			}
		} // CheckRejectAfterLacr

		private void CheckLateLoans() {
			if (MetaData.LateLoanCount > 0) {
				StepFailed<LateLoans>()
					.Init();
			} else {
				StepDone<LateLoans>()
					.Init();
			}
		} // CheckLateLoans

		private void CheckLatePayments() {
			if (Trail.MyInputData.MaxLateDays <= Trail.MyInputData.AutoReApproveMaxLatePayment) {
				StepDone<LatePayment>()
					.Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);
			} else {
				StepFailed<LatePayment>()
					.Init(Trail.MyInputData.MaxLateDays, Trail.MyInputData.AutoReApproveMaxLatePayment);
			}

			/*
			bool bHasLatePayments = false;

			foreach (Payment lp in LatePayments) {
				if (lp.IsLate(Cfg.MaxLatePayment)) {
					bHasLatePayments = true;

					lp.Fill(StepFailed<LatePayment>(), Cfg.MaxLatePayment);
				} // if
			} // for each

			if (!bHasLatePayments)
				StepDone<LatePayment>().Init(0, 0, DateTime.UtcNow, 0, DateTime.UtcNow, Cfg.MaxLatePayment);
			*/
		} // CheckLatePayments

		private void CheckNewMarketplaces() {
			if (NewMarketplaces.Count < 1) {
				StepDone<NewMarketplace>()
					.Init(NewMarketplaces.Count < 1);
				// StepDone<NewMarketplace>().Init(0, string.Empty, string.Empty, DateTime.UtcNow);
			} else {
				StepFailed<NewMarketplace>()
					.Init(NewMarketplaces.Count < 1);
				// foreach (var mp in NewMarketplaces) mp.Fill(StepFailed<NewMarketplace>());
			} // if
		} // CheckNewMarketplaces

		private void CheckOutstandingLoans() {
			if (MetaData.OpenLoanCount > Cfg.MaxNumOfOutstandingLoans) {
				StepFailed<OutstandingLoanCount>()
					.Init(MetaData.OpenLoanCount, Cfg.MaxNumOfOutstandingLoans);
			} else {
				StepDone<OutstandingLoanCount>()
					.Init(MetaData.OpenLoanCount, Cfg.MaxNumOfOutstandingLoans);
			}
		} // CheckOutstandingLoans

		private void CheckLoanCharges() {
			if (MetaData.SumOfCharges > 0) {
				StepFailed<Charges>()
					.Init(MetaData.SumOfCharges);
			} else {
				StepDone<Charges>()
					.Init(0);
			}
		} // CheckLoanCharges

		private void SetApprovedAmount() {
			if (Trail.HasDecided)
				ApprovedAmount = MetaData.ApprovedAmount;

			if (ApprovedAmount > 0) {
				StepDone<ApprovedAmount>()
					.Init(ApprovedAmount);
			} else {
				StepFailed<ApprovedAmount>()
					.Init(ApprovedAmount);
			}
		} // SetApprovedAmount

		private void CheckAvailableFunds() {
			if (ApprovedAmount < Trail.MyInputData.AvaliableFunds) {
				StepDone<EnoughFunds>()
					.Init(ApprovedAmount, Trail.MyInputData.AvaliableFunds);
			} else {
				StepFailed<EnoughFunds>()
					.Init(ApprovedAmount, Trail.MyInputData.AvaliableFunds);
			}
		} // CheckAvailableFunds

		private void ProcessRow(SafeReader sr) {
			RowType nRowType;

			string sRowType = sr["RowType"];

			if (!Enum.TryParse(sRowType, out nRowType)) {
				Log.Alert("Unsupported row type encountered: '{0}'.", sRowType);
				return;
			} // if

			switch (nRowType) {
			case RowType.MetaData:
				sr.Fill(MetaData);
				break;

			case RowType.LatePayment:
				LatePayments.Add(sr.Fill<Payment>());
				break;

			case RowType.Marketplace:
				NewMarketplaces.Add(sr.Fill<Marketplace>());
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		/// <summary>
		///     Sets overall decision to 'no approve'.
		/// </summary>
		/// <typeparam name="T">Step type.</typeparam>
		/// <returns>Step type instance for filling step details.</returns>
		private T StepFailed<T>() where T : ATrace {
			ApprovedAmount = 0;
			return Trail.Negative<T>(true);
		} // StepFailed

		/// <summary>
		///     If the step was the only step then overall decision would be 'approved'.
		/// </summary>
		/// <typeparam name="T">Step type.</typeparam>
		/// <returns>Step type instance for filling step details.</returns>
		private T StepDone<T>() where T : ATrace {
			return Trail.Affirmative<T>(false);
		} // StepFailed
	} // class Agent
} // namespace
