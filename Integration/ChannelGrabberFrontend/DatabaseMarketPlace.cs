namespace Integration.ChannelGrabberFrontend {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using Integration.ChannelGrabberConfig;

	public class DatabaseMarketPlace : DatabaseMarketplaceBase<FunctionType> {
		public DatabaseMarketPlace(string sAccountTypeName) : base(new ServiceInfo(sAccountTypeName)) {
			m_sAccountTypeName = sAccountTypeName;
		} // constructor

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new RetrieveDataHelper(helper, this);
		} // GetRetrieveDataHelper

		public override IDatabaseFunctionFactory<FunctionType> FunctionFactory {
			get {
				return new DatabaseFunctionFactory(Configuration.Instance.GetVendorInfo(m_sAccountTypeName));
			} // get
		} // FunctionFactory

		private readonly string m_sAccountTypeName;
	} // class DatabaseMarketPlace
} // namespace
