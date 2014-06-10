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
			m_nCustomerID = nCustomerID;
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

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nID = sr["ID"];
					string sDisplayName = sr["DisplayName"];
					string sType = sr["MarketplaceType"];
					byte[] oSecData = sr["SecurityData"];
					Guid oType = sr["InternalId"];

					string sSecData;

					VendorInfo vi = oCgCfg.GetVendorInfo(oType);

					if (vi == null) {
						switch (sType.Trim().ToLower()) {
						case "ekm":
							sSecData = TryDecrypt(oSecData);
							break;

						case "yodlee":
							sSecData = Serialized.Deserialize<YodleeSecurityInfo>(oSecData).Stringify();
							break;

						default:
							sSecData = oSecData.ToString();
							break;
						} // switch
					}
					else
						sSecData = TryDecrypt(oSecData);

					Log.Info(
						"Customer market place {0} - ({1}) {2}:\n{3}\n\n",
						sType,
						nID,
						sDisplayName,
						sSecData
					);

					return ActionResult.Continue;
				},
				"LoadCustomerMarketplaceSecurityData",
				new QueryParameter("CustomerID", m_nCustomerID)
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;

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
