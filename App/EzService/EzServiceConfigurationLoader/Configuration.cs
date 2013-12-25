﻿using System;
using System.Data;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzServiceConfigurationLoader {
	public class Configuration : EzServiceConfiguration.ConfigurationData {
		#region public

		#region constructor

		public Configuration(string sInstanceName, AConnection oDB, ASafeLog oLog) : base(sInstanceName) {
			m_oDB = oDB;
			m_oLog = new SafeLog(oLog);
		} // constructor

		#endregion constructor

		#endregion public

		#region protected

		#region method LoadFromDB

		protected override void LoadFromDB() {
			m_oLog.Debug("Loading service configuration from DB for service instance {0}...", RequestedInstanceName);

			DataTable oTbl = m_oDB.ExecuteReader(
				"EzServiceLoadConfiguration",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@InstanceName", RequestedInstanceName)
			);

			if (oTbl.Rows.Count != 1)
				throw new Exception(string.Format("Failed to load service configuration for service instance {0} from DB.", RequestedInstanceName));

			DataRow oRow = oTbl.Rows[0];

			InstanceID = Convert.ToInt32(oRow["InstanceID"]);
			SleepTimeout = Convert.ToInt32(oRow["SleepTimeout"]);
			AdminPort = Convert.ToInt32(oRow["AdminPort"]);
			ClientPort = Convert.ToInt32(oRow["ClientPort"]);
			HostName = oRow["HostName"].ToString();

			oTbl.Dispose();

			m_oLog.Debug("Loading service configuration from DB for service instance {0} complete.", RequestedInstanceName);
		} // LoadFromDB

		#endregion method LoadFromDB

		#region method WriteToLog

		protected override void WriteToLog() {
			m_oLog.Debug("Service configuration:");
			m_oLog.Debug("Instance ID: {0}", InstanceID);
			m_oLog.Debug("Main loop sleep time: {0}", SleepTimeout);
			m_oLog.Debug("Client endpoint address: {0}", ClientEndpointAddress);
			m_oLog.Debug("Admin endpoint address: {0}", AdminEndpointAddress);
			m_oLog.Debug("End of service configuration.");
		} // WriteToLog

		#endregion method WriteToLog

		#endregion protected

		#region private

		private readonly AConnection m_oDB;
		private readonly SafeLog m_oLog;

		#endregion private
	} // class Configuration
} // namespace EzServiceConfigurationLoader
