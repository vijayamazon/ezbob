﻿namespace EzServiceConfigurationLoader {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Configuration : EzServiceConfiguration.ConfigurationData {

		public Configuration(string sInstanceName, AConnection oDB, ASafeLog oLog) : base(sInstanceName) {
			m_oDB = oDB;
			m_oLog = new SafeLog(oLog);
		} // constructor

		protected override void LoadFromDB() {
			m_oLog.Debug("Loading service configuration from DB for service instance {0}...", RequestedInstanceName);

			SafeReader sr = m_oDB.GetFirst(
				SpName,
				CommandSpecies.StoredProcedure,
				new QueryParameter(ArgName, RequestedInstanceName)
			);

			InstanceID = sr["InstanceID"];

			if (InstanceID < 1)
				throw new Exception(string.Format("Failed to load service configuration for service instance {0} from DB.", RequestedInstanceName));

			SleepTimeout = sr["SleepTimeout"];
			AdminPort = sr["AdminPort"];
			ClientPort = sr["ClientPort"];
			HostName = sr["HostName"];
			ClientTimeoutSeconds = sr["ClientTimeoutSeconds"];

			m_oLog.Debug("Loading service configuration from DB for service instance {0} complete.", RequestedInstanceName);
		} // LoadFromDB

		protected virtual string SpName {
			get { return "EzServiceLoadConfiguration"; } // get
		} // SpName

		protected virtual string ArgName {
			get { return "@InstanceName"; } // get
		} // ArgName

		protected override void WriteToLog() {
			m_oLog.Debug("Service configuration:");
			m_oLog.Debug("Instance ID: {0}", InstanceID);
			m_oLog.Debug("Main loop sleep time: {0}", SleepTimeout);
			m_oLog.Debug("Client endpoint address: {0}", ClientEndpointAddress);
			m_oLog.Debug("Admin endpoint address: {0}", AdminEndpointAddress);
			m_oLog.Debug("Timeout (seconds): {0}", ClientTimeoutSeconds);
			m_oLog.Debug("End of service configuration.");
		} // WriteToLog

		private readonly AConnection m_oDB;
		private readonly SafeLog m_oLog;

	} // class Configuration
} // namespace EzServiceConfigurationLoader
