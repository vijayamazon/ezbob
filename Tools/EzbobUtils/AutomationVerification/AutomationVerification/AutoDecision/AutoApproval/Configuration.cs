namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System.Collections.Generic;
	using System.Reflection;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Contains auto approval configuration parameters (max and min amount to approve, silent mode, etc.).
	/// </summary>
	public class Configuration {
		#region public

		#region constructor

		public Configuration() {
			m_oAllowedCaisStatusesWithLoan = new List<string>();
			m_oAllowedCaisStatusesWithoutLoan = new List<string>();
		} // constructor

		public Configuration(AConnection oDB, ASafeLog oLog) : this() {
			m_oDB = oDB;
			m_oLog = oLog;
		} // constructor

		#endregion constructor

		#region method Load

		public void Load() {
			m_oDB.ForEachRowSafe(SetValue, "LoadApprovalConfigs", CommandSpecies.StoredProcedure);
		} // Load

		#endregion method Load

		public int ExperianScoreThreshold { get; set; }
		public int CustomerMinAge { get; set; }
		public int CustomerMaxAge { get; set; }
		public int MinTurnover1M { get; set; }
		public int MinTurnover3M { get; set; }
		public int MinTurnover1Y { get; set; }
		public int MinMPSeniorityDays { get; set; }
		public int MaxOutstandingOffers { get; set; }
		public int MaxTodayLoans { get; set; }
		public int MaxDailyApprovals { get; set; }
		public int MaxAllowedDaysLate { get; set; }
		public int MaxNumOfOutstandingLoans { get; set; }
		public decimal MinRepaidPortion { get; set; }
		public int MinAmount { get; set; }
		public int MaxAmount { get; set; }
		public bool IsSilent { get; set; }
		public string SilentTemplateName { get; set; }
		public string SilentToAddress { get; set; }
		public int BusinessScoreThreshold { get; set; }

		public int OnlineTurnoverAge { get; set; }
		public decimal OnlineTurnoverDropQuarterRatio { get; set; }
		public decimal OnlineTurnoverDropMonthRatio { get; set; }
		public int HmrcTurnoverAge { get; set; }
		public decimal HmrcTurnoverDropQuarterRatio { get; set; }
		public decimal HmrcTurnoverDropHalfYearRatio { get; set; }

		#region property AllowedCaisStatusesWithLoan

		public string AllowedCaisStatusesWithLoan {
			get { return string.Join(", ", m_oAllowedCaisStatusesWithLoan); } // get
			set { SaveCaisStatuses(value, m_oAllowedCaisStatusesWithLoan); } // set
		} // AllowedCaisStatusesWithLoan

		private readonly List<string> m_oAllowedCaisStatusesWithLoan;

		#endregion property AllowedCaisStatusesWithLoan

		#region property AllowedCaisStatusesWithoutLoan

		public string AllowedCaisStatusesWithoutLoan {
			get { return string.Join(", ", m_oAllowedCaisStatusesWithoutLoan); } // get
			set { SaveCaisStatuses(value, m_oAllowedCaisStatusesWithoutLoan); } // set
		} // AllowedCaisStatusesWithoutLoan

		private readonly List<string> m_oAllowedCaisStatusesWithoutLoan;

		#endregion property AllowedCaisStatusesWithoutLoan

		public List<string> GetAllowedCaisStatusesWithLoan() {
			return m_oAllowedCaisStatusesWithLoan;
		} // GetAllowedCaisStatusesWithLoan

		public List<string> GetAllowedCaisStatusesWithoutLoan() {
			return m_oAllowedCaisStatusesWithoutLoan;
		} // GetAllowedCaisStatusesWithoutLoan

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

			if (pi.PropertyType == typeof (bool)) {
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

			m_oLog.Debug("Auto approval configuration: '{0}' was set to {1}.", sName, pi.GetValue(this));
		} // SetValue

		#endregion method SetValue

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		private const string StandardPrefix = "AutoApprove";

		#region method SaveCaisStatuses

		private void SaveCaisStatuses(string sList, List<string> oList) {
			oList.Clear();

			if (string.IsNullOrWhiteSpace(sList))
				return;

			oList.AddRange(sList.Split(','));
		} // SaveCaisStatuses

		#endregion method SaveCaisStatuses

		#endregion private
	} // class Configuration
} // namespace
