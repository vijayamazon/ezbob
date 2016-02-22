namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Threading;
	using API;
	using SalesForceLib.Models;

	public class VipRequest : AStrategy {
		public VipRequest(int customerId, string fullName, string email, string phone) {
			this.customerId = customerId;
			this.fullName = fullName;
			this.email = email;
			this.phone = phone;
			this.mailer = new StrategiesMailer();
		} // constructor

		public override string Name { get { return "VipRequest"; } }

		public override void Execute() {
			var mailMetaData = new MailMetaData("VipRequest") {
				{ "CustomerId", this.customerId > 0
						? string.Format(
							"<a alt='Open this customer in underwriter.' " +
								"href='https://{1}/UnderWriter/Customers?customerid={0}' " +
								"title='Open this customer in underwriter.' " +
								"style='font-weight:bold;color:black;background-color:#bfffcf!important' target='_blank'" +
								">{0}</a>",
							this.customerId,
							ConfigManager.CurrentValues.Instance.UnderwriterSite.Value
						)
						: string.Empty
				},
				{ "FullName", this.fullName },
				{ "Email", this.email },
				{ "Phone", this.phone },
				new Addressee(ConfigManager.CurrentValues.Instance.VipMailReceiver, bShouldRegister: false),
			};

			this.mailer.SendMailViaMandrill(mailMetaData);

			Log.Info("VIP create update lead {0}, {1}, {2}, {3}", this.email, this.customerId, false, this.customerId == 0);

			new SalesForce.AddUpdateLeadAccount(
				this.email,
				this.customerId,
				false,
				this.customerId == 0
			).Execute();

			Thread.Sleep(40000); // Ugly fix for SF race condition.

			Log.Info("VIP add task {0}, {1}", this.email, this.customerId);

			new SalesForce.AddTask(this.customerId, new TaskModel {
				Email = this.email,
				Origin = "ezbob",
				Originator = "System",
				CreateDate = DateTime.UtcNow,
				DueDate = DateTime.UtcNow.AddDays(1),
				Subject = "VIP request",
				IsOpportunity = false,
				Description = "VIP request"
			}).Execute();
		} // Execute

		private readonly int customerId;
		private readonly string fullName;
		private readonly string email;
		private readonly string phone;
		private readonly StrategiesMailer mailer;
	} // class VipRequest
} // namespace
