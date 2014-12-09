namespace AutomationCalculator.AutoDecision.AutoApproval {
	using Ezbob.Utils.Lingvo;

	/// <summary>
	/// Auto approval result.
	/// </summary>
	public class Result {

		public Result(int nApprovedAmount, int nOfferLength, bool bIsEmailSendingBanned) {
			ApprovedAmount = nApprovedAmount;
			OfferLength = nOfferLength;
			IsEmailSendingBanned = bIsEmailSendingBanned;
		} // constructor

		public override string ToString() {
			return string.Format(
				"amount {0} for {1}, email banned: {2}",
				ApprovedAmount,
				Grammar.Number(OfferLength, "day"),
				IsEmailSendingBanned ? "yes" : "no"
			);
		} // ToString

		public int ApprovedAmount { get; private set; }

		/// <summary>
		/// In days.
		/// </summary>
		public int OfferLength { get; private set; }

		public bool IsEmailSendingBanned { get; private set; }
	} // class Result
} // namespace
