namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using System.Globalization;
	using Ezbob.Database;

	public class LoanData {
		public int LoanID { get; set; }

		[FieldName("LoanAmount")]
		public decimal Amount { get; set; }

		public DateTime IssuedTime { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"({0}: {1} on {2}",
				LoanID,
				Amount.ToString("N0"),
				IssuedTime.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
			);
		} // ToString
	} // class LoanData
} // namespace
