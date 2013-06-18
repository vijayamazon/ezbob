using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {
	public class DatabaseFunctionFactory : IDatabaseFunctionFactory<FunctionType> {
		public DatabaseFunctionFactory(VendorInfo oVendorInfo) {
			m_oVendorInfo = oVendorInfo;
		} // constructor

		public IDatabaseFunction Create(FunctionType type) {
			return FunctionStorage.GetInstance(m_oVendorInfo).GetFunction(type);
		} // Create

		public IDatabaseFunction GetById(Guid id) {
			return FunctionStorage.GetInstance(m_oVendorInfo).GetFunctionById(id);
		} // GetById

		public IEnumerable<IDatabaseFunction> GetAll() {
			return FunctionStorage.GetInstance(m_oVendorInfo).AllFunctions();
		} // GetAll

		private readonly VendorInfo m_oVendorInfo;
	} // class DatabaseFunctionFactory
} // namespace
