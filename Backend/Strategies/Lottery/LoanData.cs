namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using Ezbob.Database;

	public class LoanData {
		[FieldName("LoanAmount")]
		public decimal Amount { get; set; }

		public DateTime IssuedTime { get; set; }
	} // class LoanData
} // namespace
