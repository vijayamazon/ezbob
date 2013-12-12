namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;
	
	public abstract class MailStrategyBase
	{
		private readonly StrategiesMailer mailer = new StrategiesMailer();
		private readonly bool sendToCustomer;

		protected string Subject { get; set; }
		protected string TemplateName { get; set; }
		protected CustomerData CustomerData { get; set; }
		protected Dictionary<string, string> Variables { get; set; }
		protected int CustomerId { get; set; }

		protected MailStrategyBase(int customerId, bool sendToCustomer)
		{
			CustomerId = customerId;
			this.sendToCustomer = sendToCustomer;
		}

		public void Execute()
		{
			CustomerData = new CustomerData(CustomerId);

			SetTemplateAndSubjectAndVariables();

			if (sendToCustomer)
			{
				mailer.SendToCustomerAndEzbob(Variables, CustomerData.Mail, TemplateName, Subject);
			}
			else
			{
				mailer.SendToEzbob(Variables, TemplateName, Subject);
			}

			ActionAtEnd();
		}

		public abstract void SetTemplateAndSubjectAndVariables();

		public virtual void ActionAtEnd()
		{
			// Default implementation is empty
		}
	}
}
