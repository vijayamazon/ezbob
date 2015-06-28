namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class CaisAccount : AResultRow {
		public const string SpName = "BAR_LoadPersonalDelayData";

		public int CustomerID { get; set; }
		public DateTime DecisionTime { get; set; }
		public DateTime LastUpdatedDate { get; set; }
		public string WorstStatus { get; set; }
		public int LoanCount { get; set; }
		public string AccountStatusCodes { get; set; }
		public int MatchTo { get; set; }

		[FieldName("Balance")]
		public int? DBBalance { get; set; }

		[FieldName("CurrentDefBalance")]
		public int? DBCurrentDefBalance { get; set; }

		public int Balance {
			get { return Math.Max(DBBalance ?? 0, DBCurrentDefBalance ?? 0); } // get
		} // Balance

		public virtual bool IsLateForApprove {
			get {
				char c = WorstStatus.Length > 0 ? WorstStatus[0] : '0';

				return LoanCount > 0 ? hasLoans.Contains(c) : hasNoLoans.Contains(c);
			} // get
		} // IsLateForApprove

		public bool IsForReject {
			get {
				return
					(MatchTo == 1) &&
					!string.IsNullOrWhiteSpace(AccountStatusCodes) &&
					(LastUpdatedDate <= DecisionTime) &&
					(MiscUtils.CountMonthsBetween(LastUpdatedDate, DecisionTime) < 1);
			} // get
		} // IsForReject

		public bool IsLateForReject {
			get {
				if (!IsForReject)
					return false;

				int nMonthCount = Math.Min(CurrentValues.Instance.Reject_LateLastMonthsNum, AccountStatusCodes.Length);

				for (int i = 1; i <= nMonthCount; i++) {
					char status = AccountStatusCodes[AccountStatusCodes.Length - i];

					if (!rejectLateStatuses.Contains(status)) {
						continue;
					} // if

					int nStatus = 0;

					int.TryParse(status.ToString(CultureInfo.InvariantCulture), out nStatus);

					if (nStatus > CurrentValues.Instance.RejectionLastValidLate)
						return true;
				} // for i

				return false;
			} // get
		} // IsLateForReject

		public bool IsPersonalDefault {
			get {
				var ecdca = new ExperianConsumerDataCaisAccounts {
					AccountStatusCodes = AccountStatusCodes,
					Balance = DBBalance,
					CurrentDefBalance = DBCurrentDefBalance,
					LastUpdatedDate = LastUpdatedDate,
					MatchTo = MatchTo,
				};

				return ecdca.IsPersonalDefault(
					CurrentValues.Instance.Reject_Defaults_Amount,
					DecisionTime.AddMonths(-1 * CurrentValues.Instance.Reject_Defaults_MonthsNum)
				);
			} // get
		} // IsPersonalDefault

		private static readonly SortedSet<char> hasLoans = new SortedSet<char>(new [] { '0', '1', '2', '3', });
		private static readonly SortedSet<char> hasNoLoans = new SortedSet<char>(new [] { '0', '1', '2', });

		private static readonly SortedSet<char> rejectLateStatuses = new SortedSet<char> { '1', '2', '3', '4', '5', '6', };
	} // class CaisAccount
} // namespace
