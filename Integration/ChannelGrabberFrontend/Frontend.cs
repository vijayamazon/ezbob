using EZBob.DatabaseLib.Common;
using Integration.ChannelGrabberConfig;
using log4net;

namespace Integration.ChannelGrabberFrontend {
	#region class Frontend

	public class Frontend {
		#region public

		#region property Instance

		public static Frontend Instance {
			get { return GetInstance(); }
			private set { ms_oInstance = value; }
		} // Instance

		private static Frontend ms_oInstance;

		#endregion property Instance

		#region method GetInstance

		public static Frontend GetInstance(ILog oLog = null) {
			lock (typeof (Frontend)) {
				if (ms_oInstance != null)
					return ms_oInstance;

				ms_oInstance = new Frontend(oLog);
				return ms_oInstance;
			} // lock
		} // GetInstance

		#endregion method GetInstance

		#region method CreateAccountData

		public AccountData CreateAccountData(string sAccountTypeName) {
			VendorInfo vi = Configuration.GetVendorInfo(sAccountTypeName);
			return vi == null ? null : new AccountData(vi);
		} // CreateAccountData

		#endregion method CreateAccountData

		#region property Configuration

		public Configuration Configuration { get; private set; }

		#endregion field Configuration

		#endregion property

		#region private

		#region constructor

		private Frontend(ILog oLog) {
			Configuration = Configuration.GetInstance(oLog);
		} // Frontend

		#endregion constructor

		#endregion private
	} // class Frontend

	#endregion class Frontend
} // namespace Integration.ChannelGrabberFrontend
