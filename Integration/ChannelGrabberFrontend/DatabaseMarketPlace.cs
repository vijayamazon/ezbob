namespace Integration.ChannelGrabberFrontend {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class DatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public DatabaseMarketPlace(string sAccountTypeName)
			: base(new ServiceInfo(sAccountTypeName)) {
		} // constructor

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new RetrieveDataHelper(helper, this);
		} // GetRetrieveDataHelper
	} // class DatabaseMarketPlace
} // namespace
