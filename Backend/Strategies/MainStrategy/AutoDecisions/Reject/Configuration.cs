namespace Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.Reject {
	using System.Reflection;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Configuration {

		public Configuration(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = oLog;
			Values = new RejectionConfigs();
		} // constructor

		public virtual void Load() {
			m_oDB.ForEachRowSafe(SetValue, "LoadAutoRejectionConfigs", CommandSpecies.StoredProcedure);
		} // Load

		public RejectionConfigs Values { get; private set; }

		private void SetValue(SafeReader sr) {
			string sName = sr["Name"];

			PropertyInfo pi = this.Values.GetType().GetProperty(sName);

			if (pi == null) {
				m_oLog.Alert("Auto reject configuration: unsupported parameter found '{0}'.", sName);
				return;
			} // if

			pi.SetValue(this.Values, sr["Value"].ToType(pi.PropertyType));

			m_oLog.Debug("Auto reject configuration: '{0}' was set to {1}.", sName, pi.GetValue(this.Values));
		} // SetValue

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

	} // class Configuration
} // namespace
