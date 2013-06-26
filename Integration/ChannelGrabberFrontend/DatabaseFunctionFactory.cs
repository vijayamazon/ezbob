using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using Integration.ChannelGrabberConfig;
using log4net;

namespace Integration.ChannelGrabberFrontend {
	public class DatabaseFunctionFactory : IDatabaseFunctionFactory<FunctionType> {
		public DatabaseFunctionFactory(VendorInfo oVendorInfo) {
			m_oVendorInfo = oVendorInfo;
			ms_oLog.DebugFormat("DatabaseFunctionFactory constructed for {0}", m_oVendorInfo.Name);
		} // constructor

		public IDatabaseFunction Create(FunctionType type) {
			ms_oLog.DebugFormat("start {0}", m_oVendorInfo.Name);

			var i = FunctionStorage.GetInstance(m_oVendorInfo).GetFunction(type);

			ms_oLog.DebugFormat("end {0}", m_oVendorInfo.Name);

			return i;
		} // Create

		public IDatabaseFunction GetById(Guid id) {
			ms_oLog.DebugFormat("start {0}", m_oVendorInfo.Name);

			var i = FunctionStorage.GetInstance(m_oVendorInfo).GetFunctionById(id);

			ms_oLog.DebugFormat("end {0}", m_oVendorInfo.Name);

			return i;
		} // GetById

		public IEnumerable<IDatabaseFunction> GetAll() {
			ms_oLog.DebugFormat("start {0}", m_oVendorInfo.Name);

			var i = FunctionStorage.GetInstance(m_oVendorInfo).AllFunctions();

			ms_oLog.DebugFormat("end {0}", m_oVendorInfo.Name);

			return i;
		} // GetAll

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(DatabaseFunctionFactory));
		private readonly VendorInfo m_oVendorInfo;
	} // class DatabaseFunctionFactory
} // namespace
