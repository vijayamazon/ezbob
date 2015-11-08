namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using global::Integration.ChannelGrabberConfig;
	using YodleeLib.connector;

	using ChaGraConfig = global::Integration.ChannelGrabberConfig.Configuration;

	public class DisplayMarketplaceSecurityData : AStrategy {

		public DisplayMarketplaceSecurityData(int nCustomerID) {
			m_oStra = new LoadCustomerMarketplaceSecurityData(nCustomerID);
		} // constructor

		public override string Name {
			get { return "Display customer marketplace security data"; }
		} // Name

		public override void Execute() {
			ChaGraConfig oCgCfg = ChaGraConfig.GetInstance(Log);

			m_oStra.Execute();

			foreach (var oRes in m_oStra.Result) {
				string sSecData;

				VendorInfo vi = oCgCfg.GetVendorInfo(oRes.MarketplaceType);

				if (vi == null) {
					switch (oRes.MarketplaceType.Trim().ToLower()) {
					case "ekm":
						sSecData = TryDecrypt(oRes.SecurityData);
						break;

					case "yodlee":
						sSecData = Serialized.Deserialize<YodleeSecurityInfo>(oRes.SecurityData).Stringify();
						break;

					default:
						sSecData = oRes.SecurityData.ToString();
						break;
					} // switch
				}
				else
					sSecData = TryDecrypt(oRes.SecurityData);

				Log.Info(
					"Customer market place {0} - ({1}) {2}:\n{3}\n\n",
					oRes.MarketplaceType,
					oRes.CustomerMarketplaceID,
					oRes.DisplayName,
					sSecData
				);
			} // for each result
		} // Execute

		private readonly LoadCustomerMarketplaceSecurityData m_oStra;

		private string TryDecrypt(byte[] oEncoded) {
			try {
				return Encrypted.Decrypt(oEncoded);
			}
			catch (Exception) {
				return "FAILED TO DECRYPT";
			} // try
		} // TryDecrypt

	} // class DisplayMarketplaceSecurityData
} // namespace Ezbob.Backend.Strategies.Misc
