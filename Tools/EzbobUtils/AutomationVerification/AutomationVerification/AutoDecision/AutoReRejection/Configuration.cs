namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System.Reflection;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Configuration {
		#region public

		#region constructor

		public Configuration(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = oLog;
		} // constructor

		#endregion constructor

		#region method Load

		public void Load() {
			m_oDB.ForEachRowSafe(SetValue, "LoadAutoRerejectConfigs", CommandSpecies.StoredProcedure);
		} // Load

		#endregion method Load

		public int MaxLRDAge { get; set; }
		public decimal MinRepaidPortion { get; set; }

		#endregion public

		#region private

		#region method SetValue

		private void SetValue(SafeReader sr) {
			string sName = sr["Name"];

			if (sName.StartsWith(StandardPrefix))
				sName = sName.Substring(StandardPrefix.Length);

			PropertyInfo pi = this.GetType().GetProperty(
				sName,
				BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
			);

			if (pi == null) {
				m_oLog.Alert("Auto approval configuration: unsupported parameter found '{0}'.", sName);
				return;
			} // if

			pi.SetValue(this, sr["Value"].ToType(pi.PropertyType));

			m_oLog.Debug("Auto re-rejection configuration: '{0}' was set to {1}.", sName, pi.GetValue(this));
		} // SetValue

		#endregion method SetValue

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		private const string StandardPrefix = "AutoReReject";

		#endregion private
	} // class Configuration
} // namespace
