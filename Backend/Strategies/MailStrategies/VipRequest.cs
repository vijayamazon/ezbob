namespace Ezbob.Backend.Strategies.MailStrategies
{
	using System;
	using System.Threading;
	using API;
	using SalesForceLib.Models;

	public class VipRequest : AStrategy
	{
		private readonly int customerId;
		private readonly string fullName;
		private readonly string email;
		private readonly string phone;
		private MailMetaData mailMetaData;
		private readonly StrategiesMailer mailer;

		public VipRequest(int customerId, string fullName, string email, string phone) {
			this.customerId = customerId;
			this.fullName = fullName;
			this.email = email;
			this.phone = phone;
			this.mailer = new StrategiesMailer();
		}

		public override string Name { get { return "VipRequest"; } }

		public override void Execute()
		{
			SetTemplateAndVariables();
			this.mailer.SendMailViaMandrill(this.mailMetaData);

		    Log.Info("VIP create update lead {0}, {1}, {2}, {3}", this.email, this.customerId, false, this.customerId == 0);
			SalesForce.AddUpdateLeadAccount addLead = new SalesForce.AddUpdateLeadAccount(this.email, this.customerId, false, this.customerId == 0);
			addLead.Execute();
			Thread.Sleep(40000); //ugly fix for SF race condition

            Log.Info("VIP add task {0}, {1}", this.email, this.customerId);
			SalesForce.AddTask addTask = new SalesForce.AddTask(this.customerId, new TaskModel {
				Email = this.email,
				Originator = "System",
				CreateDate = DateTime.UtcNow,
				DueDate = DateTime.UtcNow.AddDays(1),
				Subject = "VIP request",
				IsOpportunity = false,
                Description = "VIP request"
			});
			addTask.Execute();
		}

		protected void SetTemplateAndVariables()
		{
			this.mailMetaData = new MailMetaData("VipRequest");

			if (this.customerId > 0)
			{
				this.mailMetaData.Add("CustomerId",
								  string.Format(
									  "<a alt='Open this customer in underwriter.' href='https://{1}/UnderWriter/Customers?customerid={0}' title='Open this customer in underwriter.' style='font-weight:bold;color:black;background-color:#bfffcf!important' target='_blank'>{0}</a>",
									  this.customerId, ConfigManager.CurrentValues.Instance.UnderwriterSite.Value));
			}
			else
			{
				this.mailMetaData.Add("CustomerId", string.Empty);
			}

			this.mailMetaData.Add("FullName", this.fullName);
			this.mailMetaData.Add("Email", this.email);
			this.mailMetaData.Add("Phone", this.phone);

			this.mailMetaData.Add(new Addressee(ConfigManager.CurrentValues.Instance.VipMailReceiver, bShouldRegister: false));
		} // SetTemplateAndVariables

	} // class 
} // namespace
