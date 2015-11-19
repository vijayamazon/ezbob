namespace Ezbob.Backend.Strategies.ManualDecision {
	internal enum ChangeDecisionOption {
		/// <summary>
		/// Manual decision can be applied.
		/// </summary>
		Available,

		/// <summary>
		/// Manual decision cannot be applied (this is not bad): there is no cash request for customer
		/// (e.g. customer didn't complete wizard). Wait and refresh browser page to continue.
		/// </summary>
		BlockedNoCashRequest,

		/// <summary>
		/// Manual decision cannot be applied (this is not bad): someone else is/was working on the same customer.
		/// Refresh browser page to continue.
		/// </summary>
		BlockedByConcurrency,

		/// <summary>
		/// Manual decision cannot be applied (this is not bad): someone else has already decided.
		/// Create new credit line to override.
		/// </summary>
		BlockedByFinalDecision,

		/// <summary>
		/// Manual decision cannot be applied (this is not bad): main strategy is being executed now on this customer.
		/// Wait and refresh browser page to continue.
		/// </summary>
		BlockedByMainStrategy,

		/// <summary>
		/// Manual decision cannot be applied (this is not bad): approved amount not specified.
		/// Specify approved amount to continue.
		/// </summary>
		BlockedByApprovedAmount,

		/// <summary>
		/// Manual decision cannot be applied (this is not bad): approved amount is greater than max allowed loan amount.
		/// Amend approved amount to continue.
		/// </summary>
		BlockedByMaxLoanAmount,

		/// <summary>
		/// Manual decision cannot be applied (this is bad): something went terribly not so cool.
		/// </summary>
		BlockedByError,
	} // enum ChangeDecisionOption
} // namespace
