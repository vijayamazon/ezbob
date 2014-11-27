namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject {
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
			m_oDB.ForEachRowSafe(SetValue, "LoadRejectionConfigs", CommandSpecies.StoredProcedure);
		} // Load

		#endregion method Load

		public int AutoRejectionException_AnualTurnover { get; set; }
		public int AutoRejectionException_CreditScore { get; set; }
		public int LowCreditScore { get; set; }
		public int Reject_Defaults_AccountsNum { get; set; }
		public int Reject_Defaults_Amount { get; set; }
		public int Reject_Defaults_CompanyAccountsNum { get; set; }
		public int Reject_Defaults_CompanyAmount { get; set; }
		public int Reject_Defaults_CompanyScore { get; set; }
		public int Reject_Defaults_CreditScore { get; set; }
		public int Reject_LateLastMonthsNum { get; set; }
		public int Reject_Minimal_Seniority { get; set; }
		public int Reject_NumOfLateAccounts { get; set; }
		public int RejectionCompanyScore { get; set; }
		public int RejectionExceptionMaxCompanyScore { get; set; }
		public int RejectionExceptionMaxCompanyScoreForMpError { get; set; }
		public int RejectionExceptionMaxConsumerScoreForMpError { get; set; }
		public int RejectionLastValidLate { get; set; }
		public int TotalAnnualTurnover { get; set; }
		public int TotalThreeMonthTurnover { get; set; }

		#endregion public

		#region private

		#region method SetValue

		private void SetValue(SafeReader sr) {
			string sName = sr["Name"];

			PropertyInfo pi = this.GetType().GetProperty(sName);

			if (pi == null) {
				m_oLog.Alert("Auto reject configuration: unsupported parameter found '{0}'.", sName);
				return;
			} // if

			pi.SetValue(this, sr["Value"].ToType(pi.PropertyType));

			m_oLog.Debug("Auto reject configuration: '{0}' was set to {1}.", sName, pi.GetValue(this));
		} // SetValue

		#endregion method SetValue

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		#endregion private
	} // class Configuration
} // namespace
