namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System.Collections.Generic;
	using System.Reflection;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Contains auto approval configuration parameters (max and min amount to approve, silent mode, etc.).
	/// </summary>
	public class Configuration {
		public Configuration() {
			m_oAllowedCaisStatusesWithLoan = new List<string>();
			m_oAllowedCaisStatusesWithoutLoan = new List<string>();
		} // constructor

		public Configuration(AConnection oDB, ASafeLog oLog) : this() {
			this.DB = oDB;
			this.Log = oLog;
		} // constructor

		public virtual void Load() {
			this.DB.ForEachRowSafe(SetValue, "LoadApprovalConfigs", CommandSpecies.StoredProcedure);
		} // Load

		public virtual int ExperianScoreThreshold { get; set; }
		public virtual int CustomerMinAge { get; set; }
		public virtual int CustomerMaxAge { get; set; }
		public virtual int MinTurnover1M { get; set; }
		public virtual int MinTurnover3M { get; set; }
		public virtual int MinTurnover1Y { get; set; }
		public virtual int MinMPSeniorityDays { get; set; }
		public virtual int MaxOutstandingOffers { get; set; }
		public virtual int MaxTodayLoans { get; set; }
		public virtual int MaxDailyApprovals { get; set; }
		public virtual int MaxAllowedDaysLate { get; set; }
		public virtual int MaxNumOfOutstandingLoans { get; set; }
		public virtual decimal MinRepaidPortion { get; set; }
		public virtual int MinAmount { get; set; }
		public virtual int MaxAmount { get; set; }
		public virtual bool IsSilent { get; set; }
		public virtual string SilentTemplateName { get; set; }
		public virtual string SilentToAddress { get; set; }
		public virtual int BusinessScoreThreshold { get; set; }

		public virtual int OnlineTurnoverAge { get; set; }
		public virtual decimal OnlineTurnoverDropQuarterRatio { get; set; }
		public virtual decimal OnlineTurnoverDropMonthRatio { get; set; }
		public virtual int HmrcTurnoverAge { get; set; }
		public virtual decimal HmrcTurnoverDropQuarterRatio { get; set; }
		public virtual decimal HmrcTurnoverDropHalfYearRatio { get; set; }

		public virtual string AllowedCaisStatusesWithLoan {
			get { return string.Join(", ", m_oAllowedCaisStatusesWithLoan); } // get
			set { SaveCaisStatuses(value, m_oAllowedCaisStatusesWithLoan); } // set
		} // AllowedCaisStatusesWithLoan

		private readonly List<string> m_oAllowedCaisStatusesWithLoan;

		public virtual string AllowedCaisStatusesWithoutLoan {
			get { return string.Join(", ", m_oAllowedCaisStatusesWithoutLoan); } // get
			set { SaveCaisStatuses(value, m_oAllowedCaisStatusesWithoutLoan); } // set
		} // AllowedCaisStatusesWithoutLoan

		public virtual List<string> GetAllowedCaisStatusesWithLoan() {
			return m_oAllowedCaisStatusesWithLoan;
		} // GetAllowedCaisStatusesWithLoan

		public virtual List<string> GetAllowedCaisStatusesWithoutLoan() {
			return m_oAllowedCaisStatusesWithoutLoan;
		} // GetAllowedCaisStatusesWithoutLoan

		private void SetValue(SafeReader sr) {
			string sName = sr["Name"];

			if (sName.StartsWith(StandardPrefix))
				sName = sName.Substring(StandardPrefix.Length);

			PropertyInfo pi = this.GetType().GetProperty(
				sName,
				BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
			);

			if (pi == null) {
				this.Log.Alert("Auto approval configuration: unsupported parameter found '{0}'.", sName);
				return;
			} // if

			if (pi.PropertyType == typeof(bool)) {
				string sValue = sr["Value"];

				switch ((sValue ?? string.Empty).Trim().ToLowerInvariant()) {
				case "1":
				case "true":
				case "yes":
					pi.SetValue(this, true);
					break;

				default:
					pi.SetValue(this, false);
					break;
				} // switch
			}
			else {
				pi.SetValue(this, sr["Value"].ToType(pi.PropertyType));
			} // if

			this.Log.Debug("Auto approval configuration: '{0}' was set to {1}.", sName, pi.GetValue(this));
		} // SetValue

		private ASafeLog Log { get; set; }
		private AConnection DB { get; set; }

		private const string StandardPrefix = "AutoApprove";

		private void SaveCaisStatuses(string sList, List<string> oList) {
			oList.Clear();

			if (string.IsNullOrWhiteSpace(sList))
				return;

			oList.AddRange(sList.Split(','));
		} // SaveCaisStatuses

		private readonly List<string> m_oAllowedCaisStatusesWithoutLoan;
	} // class Configuration
} // namespace
