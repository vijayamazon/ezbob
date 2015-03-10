namespace Ezbob.Backend.Strategies.MailStrategies
{
	using System;
	using System.Threading;
	using API;
	using SalesForceLib.Models;

	public class VipRequest : AStrategy
	{
		private readonly int _customerId;
		private readonly string _fullName;
		private readonly string _email;
		private readonly string _phone;
		private MailMetaData _mailMetaData;
		private readonly StrategiesMailer m_oMailer;

		public VipRequest(int customerId, string fullName, string email, string phone) {
			_customerId = customerId;
			_fullName = fullName;
			_email = email;
			_phone = phone;
			m_oMailer = new StrategiesMailer();
		}

		public override string Name { get { return "VipRequest"; } }

		public override void Execute()
		{
			SetTemplateAndVariables();
			m_oMailer.SendMailViaMandrill(_mailMetaData);

			SalesForce.AddUpdateLeadAccount addLead = new SalesForce.AddUpdateLeadAccount(_email, _customerId, false, _customerId == 0);
			addLead.Execute();
			Thread.Sleep(40000); //ugly fix for SF race condition
			SalesForce.AddTask addTask = new SalesForce.AddTask(_customerId, new TaskModel {
				Email = _email,
				Originator = "System",
				CreateDate = DateTime.UtcNow,
				DueDate = DateTime.UtcNow.AddDays(1),
				Subject = "VIP request"
			});
			addTask.Execute();
		}

		protected void SetTemplateAndVariables()
		{
			_mailMetaData = new MailMetaData("VipRequest");

			if (_customerId > 0)
			{
				_mailMetaData.Add("CustomerId",
								  string.Format(
									  "<a alt='Open this customer in underwriter.' href='https://{1}/UnderWriter/Customers?customerid={0}' title='Open this customer in underwriter.' style='font-weight:bold;color:black;background-color:#bfffcf!important' target='_blank'>{0}</a>",
									  _customerId, ConfigManager.CurrentValues.Instance.UnderwriterSite.Value));
			}
			else
			{
				_mailMetaData.Add("CustomerId", string.Empty);
			}

			_mailMetaData.Add("FullName", _fullName);
			_mailMetaData.Add("Email", _email);
			_mailMetaData.Add("Phone", _phone);

			_mailMetaData.Add(new Addressee(ConfigManager.CurrentValues.Instance.VipMailReceiver, bShouldRegister: false));
		} // SetTemplateAndVariables

	} // class 
} // namespace
