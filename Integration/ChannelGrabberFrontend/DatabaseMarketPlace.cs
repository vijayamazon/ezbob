using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using Integration.ChannelGrabberConfig;
using log4net;

namespace Integration.ChannelGrabberFrontend {
	public class DatabaseMarketPlace : DatabaseMarketplaceBase<FunctionType> {
		public DatabaseMarketPlace(string sAccountTypeName) : base(new ServiceInfo(sAccountTypeName)) {
			m_sAccountTypeName = sAccountTypeName;
			ms_oLog.DebugFormat("DatabaseMarketPlace({0}) constructed", sAccountTypeName);
		} // constructor

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			ms_oLog.DebugFormat("start {0}", m_sAccountTypeName);

			var r = new RetriveDataHelper(helper, this, Configuration.Instance.GetVendorInfo(m_sAccountTypeName));

			ms_oLog.DebugFormat("end {0}", m_sAccountTypeName);

			return r;
		} // GetRetrieveDataHelper

		public override IDatabaseFunctionFactory<FunctionType> FunctionFactory {
			get {
				ms_oLog.DebugFormat("start {0}", m_sAccountTypeName);

				var f = new DatabaseFunctionFactory(Configuration.Instance.GetVendorInfo(m_sAccountTypeName));

				ms_oLog.DebugFormat("end {0}", m_sAccountTypeName);

				return f;
			} // get
		} // FunctionFactory

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(DatabaseMarketPlace));

		private readonly string m_sAccountTypeName;
	} // class DatabaseMarketPlace
} // namespace
