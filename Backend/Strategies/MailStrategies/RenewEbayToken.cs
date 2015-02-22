namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class RenewEbayToken : AMailStrategyBase {

		public RenewEbayToken(int customerId, string marketplaceName, string eBayAddress) : base(customerId, true) {
			this.marketplaceName = marketplaceName;
			this.eBayAddress = eBayAddress;
		} // constructor

		public override string Name { get { return "Renew eBay Token"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Renew your eBay token";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"eBayName", marketplaceName},
				{"eBayAddress", string.Format("{0}/{1}",CustomerData.OriginSite, eBayAddress)}
			};
		} // SetTemplateAndVariables

		private readonly string marketplaceName;
		private readonly string eBayAddress;
	} // class RenewEbayToken
} // namespace
