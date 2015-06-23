namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.AutoApprove {
	using System;
	using Ezbob.Backend.Extensions;
	using Ezbob.Database;

	internal class CaisAccount {
		[FieldName("Id")]
		public long ID { get; set; }

		public DateTime? LastUpdatedDate { get; set; }

		[FieldName("Balance")]
		public int? DBBalance { get; set; }

		[FieldName("CurrentDefBalance")]
		public int? DBCurrentDefBalance { get; set; }

		public string AccountStatusCodes { get; set; }

		public int Balance {
			get { return Math.Max(DBBalance ?? 0, DBCurrentDefBalance ?? 0); }
		} // Balance

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"ID {0}, updated on {1}, balance {2}, codes {3}",
				ID,
				(LastUpdatedDate ?? DateTime.UtcNow).DateStr(),
				Balance,
				AccountStatusCodes
			);
		} // ToString

		public bool IsBad(DateTime now) {
			return CarCaisAccount.IsBad(now, LastUpdatedDate, Balance, AccountStatusCodes);
		} // IsBad
	} // class CaisAccount
} // namespace
