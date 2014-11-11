namespace AutomationCalculator.AutoDecision.AutoApproval {
	using Ezbob.Database;

	internal class AvailableFunds {
		[FieldName("AvailableFunds")]
		public decimal Available { get; set; }

		[FieldNameAttribute("ReservedAmount")]
		public decimal Reserved { get; set; }
	} // class AvailableFunds
} // namespace
