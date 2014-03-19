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

				LoadCustomerData();

				Log.Debug("setting template and variables...");
				SetTemplateAndVariables();
				Log.Debug("setting template and variables complete.");

				Log.Debug("Template name: {0}", TemplateName);
				Log.Debug("Customer data: {0}", CustomerData);

				Log.Debug("Variables:\n\t{0}", string.Join("\n\t", Variables.Select(kv => kv.Key + ": " + kv.Value)));

				Log.Debug("sending an email{0} to staff...", m_bSendToCustomer ? " to customer and" : string.Empty);

				m_oMailer.Send(TemplateName, Variables, GetCustomerEmail());

				Log.Debug("sending an email{0} to staff complete.", m_bSendToCustomer ? " to customer and" : string.Empty);

				Log.Debug("performing ActionAtEnd()...");
				ActionAtEnd();
				Log.Debug("performing ActionAtEnd() complete.");

				Log.Debug("Execute() complete.");
			}
			catch (Exception e) {
				throw new StrategyException(this, "something went terribly wrong during Execute()", e);
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

		#region method GetCustomerEmail

		protected virtual string GetCustomerEmail() {
			return m_bSendToCustomer ? null : CustomerData.Mail;
		} // GetCustomerEmail

		#endregion method GetCustomerEmail

		#region method SetTemplateAndVariables

		protected abstract void SetTemplateAndVariables();

		#endregion method SetTemplateAndVariables

		#region method ActionAtEnd

		protected virtual void ActionAtEnd() {
			Log.Debug("default action - nothing to do.");
		} // ActionAtEnd

		#endregion method ActionAtEnd

		#region method LoadCustomerData

		protected virtual void LoadCustomerData() {
			Log.Debug("loading customer data...");

			CustomerData = new CustomerData();

			CustomerData.Load(CustomerId, DB);

			Log.Debug("loading customer data complete.");
		} // LoadCustomerData

		#endregion method LoadCustomerData

		#region properties

		protected virtual string TemplateName { get; set; }
		protected virtual CustomerData CustomerData { get; set; }
		protected virtual Dictionary<string, string> Variables { get; set; }
		protected virtual int CustomerId { get; set; }

		#endregion properties

		#endregion protected

		#region private

		private readonly StrategiesMailer m_oMailer;
		private readonly bool m_bSendToCustomer;

		#endregion private
	} // class MailStrategyBase
} // namespace
