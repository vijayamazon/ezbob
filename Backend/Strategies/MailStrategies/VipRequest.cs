namespace Ezbob.Backend.Strategies.MailStrategies
{
	using API;

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
				Subject = "VIP request"
				IsOpportunity = false,
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
