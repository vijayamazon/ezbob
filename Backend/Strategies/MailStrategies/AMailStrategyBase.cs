namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using API;
	using Ezbob.Utils.Exceptions;

	public abstract class AMailStrategyBase : AStrategy {

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

		public virtual CustomerData CustomerData { get; set; }
		public virtual int CustomerId { get; set; }

		public virtual bool SendToCustomer {
			get { return m_bSendToCustomer; }
			set { m_bSendToCustomer = value; }
		} // SendToCustomer

		private bool m_bSendToCustomer;

		protected AMailStrategyBase(int customerId, bool bSendToCustomer, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			ToTrustPilot = false;
			m_oMailer = new StrategiesMailer(DB, Log);

			CustomerId = customerId;
			m_bSendToCustomer = bSendToCustomer;
			Log.Debug("initialisation complete.");
		} // constructor

		protected bool ToTrustPilot { get; set; }

		protected virtual Addressee[] GetRecipients() {
			return SendToCustomer
				? new[] { new Addressee(CustomerData.Mail, ToTrustPilot && !CustomerData.IsTest ? CurrentValues.Instance.TrustPilotBccMail : "") }
				: new Addressee[0];
		} // GetRecipients

		protected abstract void SetTemplateAndVariables();

		protected virtual void ActionAtEnd() {
			Log.Debug("default action - nothing to do.");
		} // ActionAtEnd

		protected virtual void LoadRecipientData() {
			Log.Debug("Loading customer data...");

			CustomerData = new CustomerData(this, CustomerId, DB);
			CustomerData.Load();

			Log.Debug("Loading customer data complete.");
		} // LoadRecipientData

		protected void SendCostumeMail(string templateName, Dictionary<string, string> variables, Addressee[] addresses)
		{
			var meta = new MailMetaData(templateName);
			m_oMailer.Send(meta, variables, addresses);
		}

		protected virtual string TemplateName { get; set; }
		protected virtual Dictionary<string, string> Variables { get; set; }

		private readonly StrategiesMailer m_oMailer;

	} // class MailStrategyBase
} // namespace
