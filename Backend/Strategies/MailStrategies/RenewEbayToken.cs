using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class RenewEbayToken : AMailStrategyBase {
		#region constructor

		public RenewEbayToken(int customerId, string marketplaceName, string eBayAddress, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
			this.marketplaceName = marketplaceName;
			this.eBayAddress = eBayAddress;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Renew eBay Token"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Please renew your eBay token";
			TemplateName = "Mandrill - Renew your eBay token";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"eBayName", marketplaceName},
				{"eBayAddress", eBayAddress}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly string marketplaceName;
		private readonly string eBayAddress;
	} // class RenewEbayToken
} // namespace
