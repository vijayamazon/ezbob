namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject {
	using System.Reflection;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Configuration {
		#region public

		#region constructor

		public Configuration(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = oLog;
			Values = new RejectionConfigs();
		} // constructor

		#endregion constructor

		#region method Load

		public virtual void Load() {
			m_oDB.ForEachRowSafe(SetValue, "LoadAutoRejectionConfigs", CommandSpecies.StoredProcedure);
		} // Load

		#endregion method Load

		public RejectionConfigs Values { get; private set; }

		#endregion public

		#region private

		#region method SetValue

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

		#endregion method SetValue

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		#endregion private
	} // class Configuration
} // namespace
