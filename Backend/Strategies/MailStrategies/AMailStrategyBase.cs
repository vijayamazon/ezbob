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

		#region property CustomerSite

		protected virtual string CustomerSite {
			get {
				m_sCustomerSite = LoadCfgValue(m_sCustomerSite, "CustomerSite", "https://app.ezbob.com");
				return m_sCustomerSite;
			} // get
		} // CustomerSite

		private string m_sCustomerSite;

		#endregion property CustomerSite

		#region property BrokerSite

		protected virtual string BrokerSite {
			get {
				m_sBrokerSite = LoadCfgValue(m_sBrokerSite, "BrokerSite", "https://app.ezbob.com/Broker");
				return m_sBrokerSite;
			} // get
		} // CustomerSite

		private string m_sBrokerSite;

		#endregion property CustomerSite

		#region properties

		protected virtual string TemplateName { get; set; }
		protected virtual Dictionary<string, string> Variables { get; set; }

		protected virtual CustomerData CustomerData { get; set; }
		protected virtual int CustomerId { get; set; }

		#endregion properties

		#region method LoadCfgValue

		protected virtual string LoadCfgValue(string sCurrentValue, string sName, string sDefault, bool bRemoveLastSlash = true) {
			if (!string.IsNullOrWhiteSpace(sCurrentValue))
				return sCurrentValue;

			string sResult = string.Empty;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					sResult = ((string)sr["Value"] ?? string.Empty).Trim();
					return ActionResult.SkipAll;
				},
				"LoadConfigurationVariable",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CfgVarName", sName)
			);

			if (string.IsNullOrWhiteSpace(sResult))
				sResult = sDefault;

			if (bRemoveLastSlash)
				while (sResult.EndsWith("/"))
					sResult = sResult.Substring(0, sResult.Length - 1);

			return sResult;
		} // LoadCfgValue

		#endregion method LoadCfgValue

		#endregion protected

		#region private

		private readonly StrategiesMailer m_oMailer;
		private readonly bool m_bSendToCustomer;

		#endregion private
	} // class MailStrategyBase
} // namespace
