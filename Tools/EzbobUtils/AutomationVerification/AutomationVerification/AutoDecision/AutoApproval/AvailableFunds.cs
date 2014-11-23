namespace AutomationCalculator.AutoDecision.AutoApproval {
	using Ezbob.Database;

	/// <summary>
	/// Contain available funds.
	/// </summary>
	public class AvailableFunds {
		/// <summary>
		/// Money available on out bank account.
		/// </summary>
		[FieldName("AvailableFunds")]
		public decimal Available { get; set; }

		/// <summary>
		/// Sum of approved cash requests that have not been taken yet.
		/// </summary>
		[FieldNameAttribute("ReservedAmount")]
		public decimal Reserved { get; set; }
	} // class AvailableFunds
} // namespace
