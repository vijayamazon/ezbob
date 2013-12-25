using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzBob.Web.Code.ApplicationCreator {
	using System.Collections;
	using Ezbob.Logger;
	using NHibernate;
	using log4net;

	public class EzSrvCfgLoader : EzServiceConfiguration.Configuration {
		public EzSrvCfgLoader(ISession oSession, ILog oLog) : base(System.Environment.MachineName) {
			m_oSession = oSession;
			m_oLog = new SafeILog(oLog);
		} // constructor

		protected override void LoadFromDB() {
			m_oLog.Debug("Loading service configuration from DB for default service instance on machine {0}...", RequestedInstanceName);

			string sQuery = string.Format("EXECUTE EzServiceGetDefaultInstance '{0}'", RequestedInstanceName);

			IList lst = m_oSession.CreateSQLQuery(sQuery)
				.AddScalar("InstanceID", NHibernateUtil.Int32)
				.AddScalar("InstanceName", NHibernateUtil.String)
				.AddScalar("SleepTimeout", NHibernateUtil.Int32)
				.AddScalar("AdminPort", NHibernateUtil.Int32)
				.AddScalar("ClientPort", NHibernateUtil.Int32)
				.AddScalar("HostName", NHibernateUtil.String)
				.List();

			if (lst.Count != 1)
				throw new Exception(string.Format("Failed to load service configuration for default service instance on machine {0} from DB.", RequestedInstanceName));

			object[] oRow = (object [])lst[0];

			InstanceID = Convert.ToInt32(oRow[0]);
			InstanceName = oRow[1].ToString();
			SleepTimeout = Convert.ToInt32(oRow[2]);
			AdminPort = Convert.ToInt32(oRow[3]);
			ClientPort = Convert.ToInt32(oRow[4]);
			HostName = oRow[5].ToString();

			m_oLog.Debug("Loading service configuration from DB for default service instance on machine {0} complete.", RequestedInstanceName);
		} // LoadFromDB

		protected override void WriteToLog() {
			m_oLog.Debug("Service configuration:");
			m_oLog.Debug("Instance ID: {0}", InstanceID);
			m_oLog.Debug("Main loop sleep time: {0}", SleepTimeout);
			m_oLog.Debug("Client endpoint address: {0}", ClientEndpointAddress);
			m_oLog.Debug("Admin endpoint address: {0}", AdminEndpointAddress);
			m_oLog.Debug("End of service configuration.");
		} // WriteToLog

		private ISession m_oSession;
		private SafeILog m_oLog;
	} // class EzSrvCfgLoader
} // namespace
