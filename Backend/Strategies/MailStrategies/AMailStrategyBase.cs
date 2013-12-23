using System;
using System.Collections.Generic;
using System.Linq;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public abstract class AMailStrategyBase : AStrategy {
		#region public

		#region method Execute

		public override void Execute() {
			try {
				Log.Debug("Execute() started...");

				Log.Debug("loading customer data...");
				CustomerData = new CustomerData(CustomerId, DB);
				Log.Debug("loading customer data complete.");

				Log.Debug("setting template and subject variables...");
				SetTemplateAndSubjectAndVariables();
				Log.Debug("setting template and subject variables complete.");

				Log.Debug("Subject: {0}", Subject);
				Log.Debug("Template name: {0}", TemplateName);
				Log.Debug("Customer data: {0}", CustomerData);

				Log.Debug("Variables:\n\t{0}", string.Join("\n\t", Variables.Select(kv => kv.Key + ": " + kv.Value)));

				Log.Debug("sending an email{0} to staff...", sendToCustomer ? " to customer and" : string.Empty);
				if (sendToCustomer)
					mailer.SendToCustomerAndEzbob(Variables, CustomerData.Mail, TemplateName, Subject);
				else
					mailer.SendToEzbob(Variables, TemplateName, Subject);
				Log.Debug("sending an email{0} to staff complete.", sendToCustomer ? " to customer and" : string.Empty);

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

		protected AMailStrategyBase(int customerId, bool sendToCustomer, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			mailer = new StrategiesMailer(DB, Log);

			CustomerId = customerId;
			this.sendToCustomer = sendToCustomer;
			Log.Debug("initialisation complete.");
		} // constructor

		#endregion constructor

		#region method SetTemplateAndSubjectAndVariables

		protected abstract void SetTemplateAndSubjectAndVariables();

		#endregion method SetTemplateAndSubjectAndVariables

		#region method ActionAtEnd

		protected virtual void ActionAtEnd() {
			Log.Debug("default action - nothing to do.");
		} // ActionAtEnd

		#endregion method ActionAtEnd

		#region properties

		protected string Subject { get; set; }
		protected string TemplateName { get; set; }
		protected CustomerData CustomerData { get; set; }
		protected Dictionary<string, string> Variables { get; set; }
		protected int CustomerId { get; set; }

		#endregion properties

		#endregion protected

		#region private

		private readonly StrategiesMailer mailer;
		private readonly bool sendToCustomer;

		#endregion private
	} // class MailStrategyBase
} // namespace
