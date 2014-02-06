namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class RenewEbayToken : AMailStrategyBase {
		#region constructor

		public RenewEbayToken(int customerId, string marketplaceName, string eBayAddress, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			this.marketplaceName = marketplaceName;
			this.eBayAddress = eBayAddress;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Renew eBay Token"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Renew your eBay token";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"eBayName", marketplaceName},
				{"eBayAddress", eBayAddress}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		private readonly string marketplaceName;
		private readonly string eBayAddress;
	} // class RenewEbayToken
} // namespace
