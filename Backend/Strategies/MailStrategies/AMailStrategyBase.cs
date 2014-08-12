namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;

	using EzBob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Utils.Exceptions;

	public abstract class AMailStrategyBase : AStrategy {
		#region public

		#region method Execute

		public override void Execute() {
			try {
				Log.Debug("Execute() started...");

				LoadRecipientData();

				Log.Debug("Setting template and variables...");
				SetTemplateAndVariables();
				Log.Debug("Setting template and variables complete.");

				Log.Debug("Template name: {0}", TemplateName);
				Log.Debug("Customer data: {0}", CustomerData);
				Log.Debug("Variables:\n\t{0}", string.Join("\n\t", Variables.Select(kv => kv.Key + ": " + kv.Value)));

				Addressee[] aryRecipients = GetRecipients() ?? new Addressee[0];

				Log.Debug("Sending an email to staff{0}...", aryRecipients.Length > 0 ? " and " + (aryRecipients.Length) + " other recipient(s)" : string.Empty);
				m_oMailer.Send(TemplateName, Variables, aryRecipients);
				Log.Debug("Sending an email to staff{0} complete.", aryRecipients.Length > 0 ? " and " + (aryRecipients.Length) + " other recipient(s)" : string.Empty);

				Log.Debug("Performing ActionAtEnd()...");
				ActionAtEnd();
				Log.Debug("Performing ActionAtEnd() complete.");

				Log.Debug("Execute() complete.");
			}
			catch (AException) {
				throw;
			} // try
			catch (Exception e) {
				throw new StrategyAlert(this, "Something went terribly wrong during Execute().", e);
			} // try
		} // Execute

		#endregion method Execute

		#endregion public

		#region protected

		#region constructor

		protected AMailStrategyBase(int customerId, bool bSendToCustomer, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			toTrustPilot = false;
			m_oMailer = new StrategiesMailer(DB, Log);

			CustomerId = customerId;
			m_bSendToCustomer = bSendToCustomer;
			Log.Debug("initialisation complete.");
		} // constructor

		#endregion constructor

		protected virtual bool toTrustPilot { get; set; }

		#region method GetRecipients

		protected virtual Addressee[] GetRecipients() {
			return SendToCustomer
				? new[] { new Addressee(CustomerData.Mail, toTrustPilot && !CustomerData.IsTest ? CurrentValues.Instance.TrustPilotBccMail : "") }
				: new Addressee[0];
		} // GetRecipients

		#endregion method GetCustomerEmail

		#region property SendToCustomer

		protected virtual bool SendToCustomer {
			get { return m_bSendToCustomer; }
			set { m_bSendToCustomer = value; }
		} // SendToCustomer

		private bool m_bSendToCustomer;

		#endregion property SendToCustomer

		#region method SetTemplateAndVariables

		protected abstract void SetTemplateAndVariables();

		#endregion method SetTemplateAndVariables

		#region method ActionAtEnd

		protected virtual void ActionAtEnd() {
			Log.Debug("default action - nothing to do.");
		} // ActionAtEnd

		#endregion method ActionAtEnd

		#region method LoadRecipientData

		protected virtual void LoadRecipientData() {
			Log.Debug("Loading customer data...");

			CustomerData = new CustomerData(this, CustomerId, DB);
			CustomerData.Load();

			Log.Debug("Loading customer data complete.");
		} // LoadRecipientData

		#endregion method LoadRecipientData

		#region properties

		protected virtual string TemplateName { get; set; }
		protected virtual Dictionary<string, string> Variables { get; set; }

		protected virtual CustomerData CustomerData { get; set; }
		protected virtual int CustomerId { get; set; }

		#endregion properties

		#endregion protected

		#region private

		private readonly StrategiesMailer m_oMailer;

		#endregion private
	} // class MailStrategyBase
} // namespace
