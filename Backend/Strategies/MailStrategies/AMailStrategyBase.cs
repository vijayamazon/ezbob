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
		#region static constructor

		static AMailStrategyBase() {
			ms_oLock = new object();
			ms_bDefaultsAreReady = false;
		} // static constructor

		#endregion static constructor

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
			InitDefaults(); // should not be moved to static constructor

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

		#region property CustomerSite

		protected virtual string CustomerSite {
			get {
				return RemoveLastSlash(CurrentValues.Instance.CustomerSite);
			} // get
		} // CustomerSite

		#endregion property CustomerSite

		#region property BrokerSite

		protected virtual string BrokerSite {
			get {
				return RemoveLastSlash(CurrentValues.Instance.BrokerSite);
			} // get
		} // BrokerSite

		#endregion property BrokerSite

		#region property UnderwriterSite

		protected virtual string UnderwriterSite {
			get {
				return RemoveLastSlash(CurrentValues.Instance.UnderwriterSite);
			} // get
		} // UnderwriterSite

		#endregion property UnderwriterSite

		#region properties

		protected virtual string TemplateName { get; set; }
		protected virtual Dictionary<string, string> Variables { get; set; }

		protected virtual CustomerData CustomerData { get; set; }
		protected virtual int CustomerId { get; set; }

		#endregion properties

		#endregion protected

		#region private

		private readonly StrategiesMailer m_oMailer;

		private static readonly object ms_oLock;
		private static bool ms_bDefaultsAreReady;

		#region method RemoveLastSlash

		private string RemoveLastSlash(string sResult) {
			while (sResult.EndsWith("/"))
				sResult = sResult.Substring(0, sResult.Length - 1);

			return sResult;
		} // RemoveLastSlash

		#endregion method RemoveLastSlash

		#region method InitDefaults

		private static void InitDefaults() {
			if (ms_bDefaultsAreReady)
				return;

			lock (ms_oLock) {
				if (ms_bDefaultsAreReady)
					return;

				CurrentValues.Instance
					.SetDefault(ConfigManager.Variables.CustomerSite, "https://app.ezbob.com")
					.SetDefault(ConfigManager.Variables.BrokerSite, "https://app.ezbob.com/Broker");

				ms_bDefaultsAreReady = true;
			} // lock
		} // InitDefaults

		#endregion method InitDefaults

		#endregion private
	} // class MailStrategyBase
} // namespace
