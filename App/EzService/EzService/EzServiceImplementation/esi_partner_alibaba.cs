namespace EzService.EzServiceImplementation {
	using DbConstants;
	using Ezbob.Backend.Strategies.Alibaba;

	partial class EzServiceImplementation {

		public ActionMetaData DataSharing(int customerID, AlibabaBusinessType businessType, int? uwId) {
			Log.Info("ESI: customerID: {0}, finalDecision: {1}", customerID, businessType);
			ActionMetaData amd = Execute<DataSharing>(customerID, uwId, customerID, businessType);
			return amd;

		}// DataSharing


	}  // class EzServiceImplementation
} // namespace