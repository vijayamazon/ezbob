namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;

	internal class CaisAccount : AResultRow {
		public const string SpName = "BAR_LoadPersonalDelayData";

		public int CustomerID { get; set; }
		public DateTime DecisionTime { get; set; }
		public DateTime LastUpdatedDate { get; set; }
		public string WorstStatus { get; set; }
		public int LoanCount { get; set; }

		[FieldName("Balance")]
		public int? DBBalance { get; set; }

		[FieldName("CurrentDefBalance")]
		public int? DBCurrentDefBalance { get; set; }

		public int? Balance {
			get {
				if ((DBBalance == null) && (DBCurrentDefBalance == null))
					return null;

				if ((DBBalance != null) && (DBCurrentDefBalance == null))
					return DBBalance;

				if ((DBBalance == null) && (DBCurrentDefBalance != null))
					return DBCurrentDefBalance;

				// ReSharper disable PossibleInvalidOperationException
				// Null check can be skipped because of previous conditions.
				return Math.Max(DBBalance.Value, DBCurrentDefBalance.Value);
				// ReSharper restore PossibleInvalidOperationException
			} // get
		} // Balance

		public bool IsLate {
			get {
				char c = WorstStatus.Length > 0 ? WorstStatus[0] : '0';

				return LoanCount > 0 ? hasLoans.Contains(c) : hasNoLoans.Contains(c);
			} // get
		} // IsLate

		private static readonly SortedSet<char> hasLoans = new SortedSet<char>(new [] { '0', '1', '2', '3', });
		private static readonly SortedSet<char> hasNoLoans = new SortedSet<char>(new [] { '0', '1', '2', });
	} // class CaisAccount
} // namespace
