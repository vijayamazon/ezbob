namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions {
	using System;
	using AutomationCalculator.AutoDecision.AutoReRejection;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoReRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ReRejection : AAutoDecisionBase {
		public ReRejection(
			int customerId,
			long? cashRequestID,
			long? nlCashRequestID,
			string tag,
			AConnection db,
			ASafeLog log
		) {
			this.db = db;
			this.log = log.Safe();

			this.trail = new ReRejectionTrail(
				customerId,
				cashRequestID,
				nlCashRequestID,
				this.log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);

			this.trail.SetTag(tag);
		} // constructor

		public override void MakeAndVerifyDecision() {
			Agent oSecondary = null;

			try {
				RunPrimary();

				oSecondary = RunSecondary();

				WasMismatch = !this.trail.EqualsTo(oSecondary.Trail);
			} catch (Exception ex) {
				StepNoReReject<ExceptionThrown>().Init(ex);
				this.log.Error(ex, "Exception during re-rejection {0}", this.trail);
			} // try

			this.trail.Save(this.db, oSecondary == null ? null : oSecondary.Trail);
		} // MakeAndVerifyDecision

		public override bool WasException {
			get {
				if (this.trail == null)
					return false;

				return this.trail.FindTrace<ExceptionThrown>() != null;
			} // get
		} // WasException

		public override bool AffirmativeDecisionMade {
			get {
				return (this.trail != null) && this.trail.HasDecided;
			} // get
		} // AffirmativeDecisionMade

		public ReRejectionTrail Trail { get { return this.trail; } }

		private void RunPrimary() {
			this.log.Debug("Primary: checking if auto re-reject should take place for customer {0}...", this.trail.CustomerID);

			SafeReader sr = this.db.GetFirst(
				"GetCustomerDataForReRejection",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.trail.CustomerID)
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
			CheckOpenLoansRepayments();
			CheckLastDecisionWasReject();
			CheckNewMarketPlaceAdded();
			CheckLRDIsTooOld();

			this.log.Debug(
				"Primary: checking if auto re-reject should take place for customer {0} complete; {1}",
				this.trail.CustomerID,
				this.trail
			);
		} // RunPrimary

		private AutomationCalculator.AutoDecision.AutoReRejection.Agent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(
				this.trail.CustomerID,
				this.trail.CashRequestID,
				this.trail.NLCashRequestID,
				this.trail.InputData.DataAsOf,
				this.db,
				this.log
			).Init();

			oSecondary.MakeDecision();

			return oSecondary;
		} // RunSecondary

		private void CheckLastDecisionWasReject() {
			if (!this.trail.MyInputData.LastDecisionWasReject) {
				StepNoReReject<LastDecisionWasReject>()
					.Init(this.trail.MyInputData.LastDecisionWasReject);
			} else
				StepNoDecision<LastDecisionWasReject>().Init(this.trail.MyInputData.LastDecisionWasReject);
		} // CheckLastDecisionWasReject

		private void CheckNewMarketPlaceAdded() {
			if (this.trail.MyInputData.LastDecisionDate.HasValue && this.trail.MyInputData.NewDataSourceAdded) {
				StepNoReReject<MarketPlaceWasAdded>()
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
				StepNoReReject<LRDIsTooOld>()
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
				StepReReject<OpenLoans>()
					.Init(this.trail.MyInputData.NumOfOpenLoans, this.trail.MyInputData.AutoReRejectMaxAllowedLoans);
			} else {
				StepNoDecision<OpenLoans>()
					.Init(this.trail.MyInputData.NumOfOpenLoans, this.trail.MyInputData.AutoReRejectMaxAllowedLoans);
			} // if
		} // CheckNumOfOpenLoans

		private void CheckOpenLoansRepayments() {
			decimal ratio = this.trail.MyInputData.OpenLoansAmount == 0
				? 0
				: this.trail.MyInputData.PrincipalRepaymentAmount / this.trail.MyInputData.OpenLoansAmount;

			// no open loans
			if (this.trail.MyInputData.OpenLoansAmount == 0)
				StepNoDecision<OpenLoansRepayments>().Init(this.trail.MyInputData.OpenLoansAmount, 0, 0);
			else {
				if (ratio < this.trail.MyInputData.AutoReRejectMinRepaidPortion) {
					StepReReject<OpenLoansRepayments>().Init(
						this.trail.MyInputData.OpenLoansAmount,
						this.trail.MyInputData.PrincipalRepaymentAmount,
						this.trail.MyInputData.AutoReRejectMinRepaidPortion
					);
				} else {
					StepNoReReject<OpenLoansRepayments>().Init(
						this.trail.MyInputData.OpenLoansAmount,
						this.trail.MyInputData.PrincipalRepaymentAmount,
						this.trail.MyInputData.AutoReRejectMinRepaidPortion
					);
				} // if
			} // if
		} // CheckOpenLoansRepayments

		private T StepNoReReject<T>() where T : ATrace {
			return this.trail.Negative<T>(true);
		} // StepNoReReject

		private T StepReReject<T>() where T : ATrace {
			return this.trail.Affirmative<T>(true);
		} // StepReReject

		private T StepNoDecision<T>() where T : ATrace {
			return this.trail.Dunno<T>();
		} // StepNoDecision

		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly ReRejectionTrail trail;
	} // class ReRejection
} // namespace
