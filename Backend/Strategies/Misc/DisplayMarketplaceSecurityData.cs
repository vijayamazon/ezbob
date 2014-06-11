namespace EzBob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using Integration.ChannelGrabberConfig;
	using YodleeLib.connector;

	public class DisplayMarketplaceSecurityData : AStrategy {
		#region public

		#region constructor

		public DisplayMarketplaceSecurityData(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oStra = new LoadCustomerMarketplaceSecurityData(nCustomerID, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Display customer marketplace security data"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Integration.ChannelGrabberConfig.Configuration oCgCfg = Integration.ChannelGrabberConfig.Configuration.GetInstance(Log);

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

		#endregion method Execute

		#endregion public

		#region private

		private readonly LoadCustomerMarketplaceSecurityData m_oStra;

		#region method TryDecrypt

		private string TryDecrypt(byte[] oEncoded) {
			try {
				return Encrypted.Decrypt(oEncoded);
			}
			catch (Exception) {
				return "FAILED TO DECRYPT";
			} // try
		} // TryDecrypt

		#endregion method TryDecrypt

		#endregion private
	} // class DisplayMarketplaceSecurityData
} // namespace EzBob.Backend.Strategies.Misc
