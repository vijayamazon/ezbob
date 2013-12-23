using System;
using System.Data;
using Ezbob.Database;
using Ezbob.Logger;
using Newtonsoft.Json;

namespace EzService {
	#region class Configuration

	public class Configuration {
		#region public

		#region constructor

		public Configuration() {} // default constructor - for deserialization

		public Configuration(string sInstanceName, AConnection oDB, ASafeLog oLog) {
			oLog.Debug("Loading service configuration from DB for service instance {0}...", sInstanceName);

			DataTable oTbl = oDB.ExecuteReader(
				"EzServiceLoadConfiguration",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@InstanceName", sInstanceName)
			);

			if (oTbl.Rows.Count != 1)
				throw new Exception(string.Format("Failed to load service configuration for service instance {0} from DB.", sInstanceName));

			Configuration cfg = JsonConvert.DeserializeObject<Configuration>(oTbl.Rows[0][0].ToString());

			if (!cfg.IsValid())
				throw new Exception("Invalid service configuration loaded from DB.");

			SleepTimeout = cfg.SleepTimeout;
			AdminPort = cfg.AdminPort;
			ClientPort = cfg.ClientPort;
			HostName = cfg.HostName;

			oLog.Debug("Service configuration:");
			oLog.Debug("Main loop sleep time: {0}", SleepTimeout);
			oLog.Debug("Client endpoint address: {0}", GetClientEndpointAddress());
			oLog.Debug("Admin endpoint address: {0}", GetAdminEndpointAddress());
			oLog.Debug("End of service configuration.");

			oLog.Debug("Loading service configuration from DB for service instance {0} complete.", sInstanceName);
		} // constructor

		#endregion constructor

		public int SleepTimeout { get; set; }
		public int AdminPort { get; set; }
		public int ClientPort { get; set; }

		#region property HostName

		public string HostName {
			get { return m_sHostName; }
			set { m_sHostName = (value ?? string.Empty).Trim(); }
		} // HostName

		private string m_sHostName;

		#endregion property HostName

		#region method GetClientEndpointAddress

		public string GetClientEndpointAddress() {
			return "http://" + HostName + ":" + ClientPort;
		} // GetClientEndpointAddress

		#endregion method GetClientEndpointAddress

		#region method GetAdminEndpointAddress

		public string GetAdminEndpointAddress() {
			return "net.tcp://" + HostName + ":" + AdminPort;
		} // GetAdminEndpointAddress

		#endregion method GetAdminEndpointAddress

		#endregion public

		#region private

		private bool IsValid() {
			return
				!string.IsNullOrEmpty(HostName) &&
				IsPortValid(AdminPort) &&
				IsPortValid(ClientPort) &&
				(SleepTimeout > 100);
		} // IsValid

		private bool IsPortValid(int nPort) {
			return (1024 <= nPort) && (nPort <= 65535);
		} // IsPortValid

		#endregion private
	} // class Configuration

	#endregion class Configuration
} // namespace EzService
