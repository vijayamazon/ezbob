using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {
	public class DatabaseMarketPlace<TSrvInfo> : DatabaseMarketplaceBase<FunctionType> where TSrvInfo: VendorInfo, IMarketplaceServiceInfo, new() {
		public DatabaseMarketPlace() : base(new TSrvInfo()) {
			m_oVendorInfo = new TSrvInfo();
		} // constructor

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new RetriveDataHelper(helper, this, m_oVendorInfo);
		} // GetRetrieveDataHelper

		public override IDatabaseFunctionFactory<FunctionType> FunctionFactory {
			get {
				return new DatabaseFunctionFactory(m_oVendorInfo);
			} // get
		} // FunctionFactory

		private readonly VendorInfo m_oVendorInfo;
	} // class DatabaseMarketPlace
} // namespace
