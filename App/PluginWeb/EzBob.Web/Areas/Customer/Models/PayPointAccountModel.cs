namespace EzBob.Web.Areas.Customer.Models {
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils.Serialization;
	using PayPoint;

	public class PayPointAccountModel {
		public int id { get; set; }
		public string mid { get; set; }
		public string vpnPassword { get; set; }
		public string remotePassword { get; set; }
		public string displayName { get { return mid; } }

		public static PayPointAccountModel ToModel(MP_CustomerMarketPlace account) {
			var payPointSecurityInfo = Serialized.Deserialize<PayPointSecurityInfo>(account.SecurityData);

			return new PayPointAccountModel {
				id = payPointSecurityInfo.MarketplaceId,
				mid = payPointSecurityInfo.Mid,
				vpnPassword = payPointSecurityInfo.VpnPassword,
				remotePassword = payPointSecurityInfo.RemotePassword
			};
		} // ToModel
	} // class PayPointAccountModel
} // namespace