namespace YodleeLib.connector
{
	using EZBob.DatabaseLib.Common;
	using Ezbob.Utils.Security;

	public class YodleeSecurityInfo : IMarketPlaceSecurityInfo
    {
        public int MarketplaceId { get; set; }
        public string Password { get; set; }
		public string Name { get; set; }
		public long ItemId { get; set; }
		public long CsId { get; set; }
    }

	public static class YodleeSecurityInfoExt {
		public static string Stringify(this YodleeSecurityInfo oInfo) {
			if (oInfo == null)
				return string.Empty;

			return string.Format(
				"Name: {0}, ItemID: {1}, CsId: {2}, Password: {3}",
				oInfo.Name, oInfo.ItemId, oInfo.CsId, Encrypted.Decrypt(oInfo.Password)
			);
		} // Stringify
	} // class YodleeSecurityInfoExt
}
