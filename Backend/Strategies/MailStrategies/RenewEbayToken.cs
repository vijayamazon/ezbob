namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class RenewEbayToken : MailStrategyBase
	{
		private readonly string marketplaceName;
		private readonly string eBayAddress;

		public RenewEbayToken(int customerId, string marketplaceName, string eBayAddress)
			: base(customerId, true)
		{
			this.marketplaceName = marketplaceName;
			this.eBayAddress = eBayAddress;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Please renew your eBay token";
			TemplateName = "Mandrill - Renew your eBay token";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"eBayName", marketplaceName},
					{"eBayAddress", eBayAddress}
				};
		}
	}
}
