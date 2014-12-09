namespace Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval {
	using System.Reflection;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class Configuration {

		public Configuration(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = oLog;
		} // constructor

		public void Load() {
			m_oDB.ForEachRowSafe(SetValue, "LoadReApprovalConfigs", CommandSpecies.StoredProcedure);
		} // Load

		public int MaxLacrAge { get; [UsedImplicitly] private set; }
		public int MaxLatePayment { get; [UsedImplicitly] private set; }
		public int MaxNumOfOutstandingLoans { get; [UsedImplicitly] private set; }

		private void SetValue(SafeReader sr) {
			string sName = sr["Name"];

			if (sName.StartsWith(StandardPrefix))
				sName = sName.Substring(StandardPrefix.Length);

			PropertyInfo pi = this.GetType().GetProperty(sName);

			if (pi == null) {
				m_oLog.Alert("Auto re-approval configuration: unsupported parameter found '{0}'.", sName);
				return;
			} // if

			pi.SetValue(this, sr["Value"].ToType(pi.PropertyType));

			m_oLog.Debug("Auto re-approval configuration: '{0}' was set to {1}.", sName, pi.GetValue(this));
		} // SetValue

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		private const string StandardPrefix = "AutoReApprove";

	} // class Configuration
} // namespace
