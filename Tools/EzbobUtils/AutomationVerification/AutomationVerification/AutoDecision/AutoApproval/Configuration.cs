namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Newtonsoft.Json;

	/// <summary>
	///     Contains auto approval configuration parameters (max and min amount to approve, silent mode, etc.).
	/// </summary>
	public class Configuration {
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
		public virtual int MinLoan { get; set; }
		public virtual int MaxAmount { get; set; }
		public virtual bool IsSilent { get; set; }
		public virtual string SilentTemplateName { get; set; }
		public virtual string SilentToAddress { get; set; }
		public virtual int BusinessScoreThreshold { get; set; }
		public virtual decimal TurnoverDropQuarterRatio { get; set; }
		public virtual int OnlineTurnoverAge { get; set; }
		public virtual decimal OnlineTurnoverDropQuarterRatio { get; set; }
		public virtual decimal OnlineTurnoverDropMonthRatio { get; set; }
		public virtual int HmrcTurnoverAge { get; set; }
		public virtual decimal HmrcTurnoverDropQuarterRatio { get; set; }
		public virtual decimal HmrcTurnoverDropHalfYearRatio { get; set; }
		public virtual int Reject_Defaults_Amount { get; set; }
		public virtual int Reject_Defaults_MonthsNum { get; set; }

		public virtual string AllowedCaisStatusesWithLoan {
			get { return string.Join(", ", this.m_oAllowedCaisStatusesWithLoan); } // get
			set { SaveCaisStatuses(value, this.m_oAllowedCaisStatusesWithLoan); } // set
		} // AllowedCaisStatusesWithoutLoan

		public virtual string AllowedCaisStatusesWithoutLoan {
			get { return string.Join(", ", this.m_oAllowedCaisStatusesWithoutLoan); } // get
			set { SaveCaisStatuses(value, this.m_oAllowedCaisStatusesWithoutLoan); } // set
		} // AllowedCaisStatusesWithLoan

		public virtual SortedSet<string> EnabledTraces { get; private set; }

		public Configuration() {
			EnabledTraces = new SortedSet<string>();
			this.m_oAllowedCaisStatusesWithLoan = new List<string>();
			this.m_oAllowedCaisStatusesWithoutLoan = new List<string>();
		} // constructor

		public Configuration(AConnection oDB, ASafeLog oLog) : this() {
			DB = oDB;
			Log = oLog;
		} // constructor

		public virtual bool IsTraceEnabled<T>() {
			return EnabledTraces.Contains(typeof(T).FullName);
		} // IsTraceEnabled

		public virtual void Load() {
			EnabledTraces.Clear();
			DB.ForEachRowSafe(SetValue, "LoadApprovalConfigs", CommandSpecies.StoredProcedure);
		} // Load

		public virtual List<string> GetAllowedCaisStatusesWithLoan() {
			return this.m_oAllowedCaisStatusesWithLoan;
		} // GetAllowedCaisStatusesWithLoan

		public virtual List<string> GetAllowedCaisStatusesWithoutLoan() {
			return this.m_oAllowedCaisStatusesWithoutLoan;
		} // GetAllowedCaisStatusesWithoutLoan

		private ASafeLog Log { get; set; }

		private AConnection DB { get; set; }

		private enum RowType {
			Cfg,
			TraceEnabled,
		} // enum RowType

		private void SetValue(SafeReader sr) {
			RowType rt;

			if (!Enum.TryParse(sr["RowType"], out rt))
				return;

			switch (rt) {
			case RowType.Cfg:
				SetCfgValue(sr);
				break;

			case RowType.TraceEnabled:
				EnabledTraces.Add(sr["Name"]);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // SetValue

		private void SetCfgValue(SafeReader sr) {
			string sName = sr["Name"];

			if (sName.StartsWith(StandardPrefix))
				sName = sName.Substring(StandardPrefix.Length);

			PropertyInfo pi = GetType()
				.GetProperty(
					sName,
					BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
				);

			if (pi == null) {
				Log.Alert("Auto approval configuration: unsupported parameter found '{0}'.", sName);
				return;
			} // if

			if (pi.PropertyType == typeof(bool)) {
				string sValue = sr["Value"];

				switch ((sValue ?? string.Empty).Trim()
					.ToLowerInvariant()) {
					case "1":
					case "true":
					case "yes":
						pi.SetValue(this, true);
						break;

					default:
						pi.SetValue(this, false);
						break;
				} // switch
			} else
				pi.SetValue(this, sr["Value"].ToType(pi.PropertyType));

			Log.Debug("Auto approval configuration: '{0}' was set to {1}.", sName, pi.GetValue(this));
		} // SetCfgValue

		private void SaveCaisStatuses(string sList, List<string> oList) {
			oList.Clear();

			if (string.IsNullOrWhiteSpace(sList))
				return;

			oList.AddRange(sList.Split(','));
		} // SaveCaisStatuses

		private const string StandardPrefix = "AutoApprove";

		private readonly List<string> m_oAllowedCaisStatusesWithLoan;

		private readonly List<string> m_oAllowedCaisStatusesWithoutLoan;
	} // class Configuration
} // namespace
