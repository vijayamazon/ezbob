namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using MailApi;

	internal class DispatchNotifications : AOneExitStep{
		public DispatchNotifications(
			string outerContextDescription,
			bool isSilentlyApproved,
			StrategiesMailer mailer,
			MedalResult medal,
			CustomerDetails customerDetails,
			AutoDecisionResponse autoDecisionResponse,
			OfferResult offerResult,
			Guid? approvalTrailUniqueID,
			string silentEmailRecipient,
			string silentEmailSenderName,
			string silentEmailSenderAddress
		) : base(outerContextDescription) {
			this.isSilentlyApproved = isSilentlyApproved;
			this.mailer = mailer;
			this.medal = medal;
			this.customerDetails = customerDetails;
			this.autoDecisionResponse = autoDecisionResponse;
			this.offerResult = offerResult;
			this.approvalTrailUniqueID = approvalTrailUniqueID;
			this.silentEmailRecipient = silentEmailRecipient;
			this.silentEmailSenderName = silentEmailSenderName;
			this.silentEmailSenderAddress = silentEmailSenderAddress;
		} // constructor

		protected override void ExecuteStep() {
			if (this.isSilentlyApproved)
				NotifyAutoApproveSilentMode();

			bool sendToCustomer =
				!this.customerDetails.FilledByBroker || (this.customerDetails.NumOfPreviousApprovals != 0);

			var notifier = new ExternalNotifier(
				this.mailer,
				this.customerDetails.ID,
				this.medal,
				this.customerDetails,
				this.autoDecisionResponse,
				sendToCustomer,
				Log
			);

			notifier.Execute();
		} // ExecuteStep

		private void NotifyAutoApproveSilentMode() {
			if (this.offerResult == null)
				return;

			int autoApprovedAmount = this.offerResult.Amount;
			int repaymentPeriod = this.offerResult.Period;
			decimal interestRate = this.offerResult.InterestRate / 100M;
			decimal setupFee = this.offerResult.SetupFee / 100M;

			try {
				var message = string.Format(
					@"<h1><u>Silent auto approve for customer <b style='color:red'>{0}</b></u></h1><br>
					<h2><b>Offer:</b></h2>
					<pre><h3>Amount: {1}</h3></pre><br>
					<pre><h3>Period: {2}</h3></pre><br>
					<pre><h3>Interest rate: {3}</h3></pre><br>
					<pre><h3>Setup fee: {4}</h3></pre><br>
					<h2><b>Decision flow ID:</b></h2>
					<pre><h3>{5}</h3></pre><br>",
					this.customerDetails.ID,
					autoApprovedAmount.ToString("C0", Strategies.Library.Instance.Culture),
					repaymentPeriod,
					interestRate.ToString("P2", Strategies.Library.Instance.Culture),
					setupFee.ToString("P2", Strategies.Library.Instance.Culture),
					this.approvalTrailUniqueID == null
						? "N/A"
						: this.approvalTrailUniqueID.Value.ToString().ToUpperInvariant()
				);

				new Mail().Send(
					this.silentEmailRecipient,
					null,
					message,
					this.silentEmailSenderAddress,
					this.silentEmailSenderName,
					"#SilentApprove for customer " + this.customerDetails.ID
				);
			} catch (Exception e) {
				Log.Alert(e, "Failed sending silent auto approval email for {0}.", OuterContextDescription);
			} // try
		} // NotifyAutoApproveSilentMode

		private readonly bool isSilentlyApproved;
		private readonly StrategiesMailer mailer;
		private readonly MedalResult medal;
		private readonly CustomerDetails customerDetails;
		private readonly AutoDecisionResponse autoDecisionResponse;
		private readonly OfferResult offerResult;
		private readonly Guid? approvalTrailUniqueID;
		private readonly string silentEmailRecipient;
		private readonly string silentEmailSenderName;
		private readonly string silentEmailSenderAddress;
	} // class DispatchNotifications
} // namespace
