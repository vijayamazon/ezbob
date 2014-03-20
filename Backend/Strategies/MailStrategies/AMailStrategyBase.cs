namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;

	using EzBob.Backend.Strategies.MailStrategies.API;

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

				string[] aryRecipients = GetRecipients() ?? new string[0];

				Log.Debug("Sending an email to staff{0}...", aryRecipients.Length > 0 ? " and " + (aryRecipients.Length) + " other recipient(s)" : string.Empty);
				m_oMailer.Send(TemplateName, Variables, aryRecipients);
				Log.Debug("Sending an email to staff{0} complete.", aryRecipients.Length > 0 ? " and " + (aryRecipients.Length) + " other recipient(s)" : string.Empty);

				Log.Debug("Performing ActionAtEnd()...");
				ActionAtEnd();
				Log.Debug("Performing ActionAtEnd() complete.");

				Log.Debug("Execute() complete.");
			}
			catch (Exception e) {
				throw new StrategyException(this, "Something went terribly wrong during Execute() of " + this.GetType(), e);
			} // try
		} // Execute

		#endregion method Execute

		#endregion public

		#region protected

		#region constructor

		protected AMailStrategyBase(int customerId, bool bSendToCustomer, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oMailer = new StrategiesMailer(DB, Log);

			CustomerId = customerId;
			m_bSendToCustomer = bSendToCustomer;
			Log.Debug("initialisation complete.");
		} // constructor

		#endregion constructor

		#region method GetRecipients

		protected virtual string[] GetRecipients() {
			return m_bSendToCustomer ? new string[] { CustomerData.Mail } : new string[0];
		} // GetRecipients

		#endregion method GetCustomerEmail

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

			CustomerData = new CustomerData();

			CustomerData.Load(CustomerId, DB);

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
		private readonly bool m_bSendToCustomer;

		#endregion private
	} // class MailStrategyBase
} // namespace
