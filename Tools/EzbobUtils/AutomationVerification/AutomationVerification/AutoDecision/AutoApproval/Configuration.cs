namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System.Collections.Generic;
	using System.Reflection;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	internal class Configuration {
		#region public

		#region constructor

		public Configuration(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = oLog;

			m_oAllowedCaisStatusesWithLoan = new List<string>();
			m_oAllowedCaisStatusesWithoutLoan = new List<string>();
		} // constructor

		#endregion constructor

		#region method Load

		public void Load() {
			m_oDB.ForEachRowSafe(SetValue, "LoadApprovalConfigs", CommandSpecies.StoredProcedure);
		} // Load

		#endregion method Load

		public int ExperianScoreThreshold { get; [UsedImplicitly] private set; }
		public int CustomerMinAge { get; [UsedImplicitly] private set; }
		public int CustomerMaxAge { get; [UsedImplicitly] private set; }
		public int MinTurnover1M { get; [UsedImplicitly] private set; }
		public int MinTurnover3M { get; [UsedImplicitly] private set; }
		public int MinTurnover1Y { get; [UsedImplicitly] private set; }
		public int MinMPSeniorityDays { get; [UsedImplicitly] private set; }
		public int MaxOutstandingOffers { get; [UsedImplicitly] private set; }
		public int MaxTodayLoans { get; [UsedImplicitly] private set; }
		public int MaxDailyApprovals { get; [UsedImplicitly] private set; }
		public int MaxAllowedDaysLate { get; [UsedImplicitly] private set; }
		public int MaxNumOfOutstandingLoans { get; [UsedImplicitly] private set; }
		public decimal MinRepaidPortion { get; [UsedImplicitly] private set; }
		public int MinAmount { get; [UsedImplicitly] private set; }
		public int MaxAmount { get; [UsedImplicitly] private set; }
		public bool IsSilent { get; [UsedImplicitly] private set; }
		public string SilentTemplateName { get; [UsedImplicitly] private set; }
		public string SilentToAddress { get; [UsedImplicitly] private set; }

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

			PropertyInfo pi = this.GetType().GetProperty(sName);

			if (pi == null) {
				m_oLog.Alert("Auto approval configuration: unsupported parameter found '{0}'.", sName);
				return;
			} // if

			pi.SetValue(this, sr["Value"].ToType(pi.PropertyType));

			m_oLog.Debug("Auto approval configuration: '{0}' was set to {1}.", sName, pi.GetValue(this));
		} // SetValue

		#endregion method SetValue

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		private const string StandardPrefix = "AutoApprove";

		#region property AllowedCaisStatusesWithLoan

		[UsedImplicitly]
		private string AllowedCaisStatusesWithLoan {
			get { return string.Join(", ", m_oAllowedCaisStatusesWithLoan); } // get
			set { SaveCaisStatuses(value, m_oAllowedCaisStatusesWithLoan); } // set
		} // AllowedCaisStatusesWithLoan

		#endregion property AllowedCaisStatusesWithLoan

		#region property AllowedCaisStatusesWithoutLoan

		[UsedImplicitly]
		private string AllowedCaisStatusesWithoutLoan {
			get { return string.Join(", ", m_oAllowedCaisStatusesWithoutLoan); } // get
			set { SaveCaisStatuses(value, m_oAllowedCaisStatusesWithoutLoan); } // set
		} // AllowedCaisStatusesWithoutLoan

		#endregion property AllowedCaisStatusesWithoutLoan

		#region method SaveCaisStatuses

		private void SaveCaisStatuses(string sList, List<string> oList) {
			oList.Clear();

			if (string.IsNullOrWhiteSpace(sList))
				return;

			oList.AddRange(sList.Split(','));
		} // SaveCaisStatuses

		#endregion method SaveCaisStatuses

		private readonly List<string> m_oAllowedCaisStatusesWithLoan;
		private readonly List<string> m_oAllowedCaisStatusesWithoutLoan;

		#endregion private
	} // class Configuration
} // namespace
