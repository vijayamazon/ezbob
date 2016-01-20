namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	using System;
	using System.Reflection;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Configuration {

		public Configuration(AConnection oDB, ASafeLog oLog) {
			this.m_oDB = oDB;
			this.m_oLog = oLog;
			Values = new RejectionConfigs();
		} // constructor

		public virtual void Load() {
			Values.EnabledTraces.Clear();
			this.m_oDB.ForEachRowSafe(SetValue, "LoadAutoRejectionConfigs", CommandSpecies.StoredProcedure);
		} // Load

		public RejectionConfigs Values { get; private set; }

		private void SetValue(SafeReader sr) {
			string rowType = sr["RowType"];

			RowType rt;

			if (!Enum.TryParse(rowType, true, out rt)) {
				this.m_oLog.Alert("Unknown row type: {0}", rowType);
				return;
			} // if

			string sName = sr["Name"];

			switch (rt) {
			case RowType.Cfg:
				PropertyInfo pi = this.Values.GetType().GetProperty(sName);

				if (pi == null) {
					this.m_oLog.Alert("Auto reject configuration: unsupported parameter found '{0}'.", sName);
					return;
				} // if

				pi.SetValue(this.Values, sr["Value"].ToType(pi.PropertyType));

				this.m_oLog.Debug("Auto reject configuration: '{0}' was set to {1}.", sName, pi.GetValue(this.Values));
				break;

			case RowType.TraceEnabled:
				Values.EnabledTraces.Add(sName);
				this.m_oLog.Debug("Auto reject configuration: '{0}' trace is enabled.", sName);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch

		} // SetValue

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		private enum RowType {
			Cfg,
			TraceEnabled,
		} // enum RowType
	} // class Configuration
} // namespace
