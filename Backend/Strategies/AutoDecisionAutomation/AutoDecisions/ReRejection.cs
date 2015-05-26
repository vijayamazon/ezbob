namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions {
	using System;
	using AutomationCalculator.AutoDecision.AutoReRejection;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoReRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class ReRejection {
		public ReRejection(int customerId, AConnection db, ASafeLog log) {
			this.db = db;
			this.log = log.Safe();
			this.customerId = customerId;

			this.trail = new ReRejectionTrail(
				customerId,
				this.log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);
		} // constructor

		public bool MakeAndVerifyDecision(string tag = null) {
		    this.trail.SetTag(tag);

			RunPrimary();

			Agent oSecondary = RunSecondary();

			bool bSuccess = this.trail.EqualsTo(oSecondary.Trail);

			this.trail.Save(this.db, oSecondary.Trail, tag: tag);

			return bSuccess;
		} // MakeAndVerifyDecision

		public void MakeDecision(AutoDecisionResponse response, string tag) {
			try {
				if (MakeAndVerifyDecision(tag) && this.trail.HasDecided) {
					response.Decision = DecisionActions.ReReject;
					response.AutoRejectReason = "Auto Re-Reject";
					response.CreditResult = CreditResultStatus.Rejected;
					response.UserStatus = Status.Rejected;
					response.SystemDecision = SystemDecision.Reject;
					response.DecisionName = "Re-rejection";
				} // if
			} catch (Exception ex) {
				StepNoReReject<ExceptionThrown>(true).Init(ex);
				this.log.Error(ex, "Exception during re-rejection {0}", this.trail);
			} // try
		} // MakeDecision

		private void RunPrimary() {
			this.log.Debug("Primary: checking if auto re-reject should take place for customer {0}...", this.customerId);

			SafeReader sr = this.db.GetFirst(
				"GetCustomerDataForReRejection",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			bool lastDecisionWasReject = sr["LastDecisionWasReject"];
			DateTime? lastRejectDate = sr["LastRejectDate"];
			DateTime? lastDecisionDate = sr["LastDecisionDate"];
			bool newDataSourceAdded = sr["NewDataSourceAdded"];
			int openLoansAmount = sr["OpenLoansAmount"];
			decimal principalRepaymentAmount = sr["PrincipalRepaymentAmount"];
			int numOfOpenLoans = sr["NumOfOpenLoans"];

			this.trail.MyInputData.Init(DateTime.UtcNow,
				lastDecisionWasReject,
				lastRejectDate,
				lastDecisionDate,
				newDataSourceAdded,
				openLoansAmount,
				principalRepaymentAmount,
				numOfOpenLoans,
				CurrentValues.Instance.AutoReRejectMinRepaidPortion,
				CurrentValues.Instance.AutoReRejectMaxLRDAge,
				CurrentValues.Instance.AutoReRejectMaxAllowedLoans
			);

			CheckNumOfOpenLoans();
			CheckLastDecisionWasReject();
			CheckNewMarketPlaceAdded();
			CheckLRDIsTooOld();
			CheckOpenLoansRepayments();

			this.log.Debug(
				"Primary: checking if auto re-reject should take place for customer {0} complete; {1}",
				this.customerId,
				this.trail
			);
		} // RunPrimary

		private AutomationCalculator.AutoDecision.AutoReRejection.Agent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(
				this.customerId, this.trail.InputData.DataAsOf, this.db, this.log
			).Init();

			oSecondary.MakeDecision();

			return oSecondary;
		} // RunSecondary

		private void CheckLastDecisionWasReject() {
			if (!this.trail.MyInputData.LastDecisionWasReject) {
				StepNoReReject<LastDecisionWasReject>(bLockDecisionAfterAddingAStep: true)
					.Init(this.trail.MyInputData.LastDecisionWasReject);
			} else
				StepNoDecision<LastDecisionWasReject>().Init(this.trail.MyInputData.LastDecisionWasReject);
		} // CheckLastDecisionWasReject

		private void CheckNewMarketPlaceAdded() {
			if (this.trail.MyInputData.LastDecisionDate.HasValue && this.trail.MyInputData.NewDataSourceAdded) {
				StepNoReReject<MarketPlaceWasAdded>(bLockDecisionAfterAddingAStep: true)
					.Init(this.trail.MyInputData.NewDataSourceAdded);
			} else
				StepNoDecision<MarketPlaceWasAdded>().Init(this.trail.MyInputData.NewDataSourceAdded);
		} // CheckNewMarketPlaceAdded

		private void CheckLRDIsTooOld() {
			bool noReject = this.trail.MyInputData.LastRejectDate.HasValue &&
				(decimal)(
					this.trail.MyInputData.DataAsOf - this.trail.MyInputData.LastRejectDate.Value
				).TotalDays > this.trail.MyInputData.AutoReRejectMaxLRDAge;

			if (noReject) {
				StepNoReReject<LRDIsTooOld>(bLockDecisionAfterAddingAStep: true)
					.Init(
						(decimal)(
							this.trail.MyInputData.DataAsOf - this.trail.MyInputData.LastRejectDate.Value
						).TotalDays,
						this.trail.MyInputData.AutoReRejectMaxLRDAge
					);
			} else {
				var days = this.trail.MyInputData.LastRejectDate.HasValue
					? (decimal)(
						this.trail.MyInputData.DataAsOf - this.trail.MyInputData.LastRejectDate.Value
					).TotalDays
					: 0.0M;

				StepNoDecision<LRDIsTooOld>().Init(days, this.trail.MyInputData.AutoReRejectMaxLRDAge);
			} // if
		} // CheckLRDIsTooOld

		private void CheckNumOfOpenLoans() {
			if (this.trail.MyInputData.NumOfOpenLoans >= this.trail.MyInputData.AutoReRejectMaxAllowedLoans) {
				StepReReject<OpenLoans>(bLockDecisionAfterAddingAStep: true)
					.Init(this.trail.MyInputData.NumOfOpenLoans, this.trail.MyInputData.AutoReRejectMaxAllowedLoans);
			} else {
				StepNoDecision<OpenLoans>()
					.Init(this.trail.MyInputData.NumOfOpenLoans, this.trail.MyInputData.AutoReRejectMaxAllowedLoans);
			}
		} // CheckNumOfOpenLoans

		private void CheckOpenLoansRepayments() {
			decimal ratio = this.trail.MyInputData.OpenLoansAmount == 0
				? 0
				: this.trail.MyInputData.PrincipalRepaymentAmount / this.trail.MyInputData.OpenLoansAmount;

			// no open loans
			if (this.trail.MyInputData.OpenLoansAmount == 0) {
				StepNoDecision<OpenLoansRepayments>().Init(this.trail.MyInputData.OpenLoansAmount, 0, 0);
			} else {
				if (ratio < this.trail.MyInputData.AutoReRejectMinRepaidPortion) {
					StepReReject<OpenLoansRepayments>(true).Init(
						this.trail.MyInputData.OpenLoansAmount,
						this.trail.MyInputData.PrincipalRepaymentAmount,
						this.trail.MyInputData.AutoReRejectMinRepaidPortion
						);
				} else {
					StepNoReReject<OpenLoansRepayments>(true).Init(
						this.trail.MyInputData.OpenLoansAmount,
						this.trail.MyInputData.PrincipalRepaymentAmount,
						this.trail.MyInputData.AutoReRejectMinRepaidPortion
						);
				} // if
			} // if
		} // CheckOpenLoansRepayments

		private T StepNoReReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return this.trail.Negative<T>(bLockDecisionAfterAddingAStep);
		} // StepNoReReject

		private T StepReReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return this.trail.Affirmative<T>(bLockDecisionAfterAddingAStep);
		} // StepReReject

		private T StepNoDecision<T>() where T : ATrace {
			return this.trail.Dunno<T>();
		} // StepNoDecision

		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly int customerId;
		private readonly ReRejectionTrail trail;
	} // class ReRejection
} // namespace
