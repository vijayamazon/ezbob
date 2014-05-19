namespace EzBob.Web.Areas.Customer.Models
{
	using CommonLib;
	using EZBob.DatabaseLib.Model.Database;
	using PayPoint;

	public class PayPointAccountModel
	{
		public int id { get; set; }
		public string mid { get; set; }
		public string vpnPassword { get; set; }
		public string remotePassword { get; set; }
		public string displayName { get { return mid; } }

		public static PayPointAccountModel ToModel(MP_CustomerMarketPlace account)
		{
			var payPointSecurityInfo = SerializeDataHelper.DeserializeType<PayPointSecurityInfo>(account.SecurityData);

			return new PayPointAccountModel
			{
				id = payPointSecurityInfo.MarketplaceId,
				mid = payPointSecurityInfo.Mid,
				vpnPassword = payPointSecurityInfo.VpnPassword,
				remotePassword = payPointSecurityInfo.RemotePassword
			};
		}
	}
}