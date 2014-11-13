namespace AutomationCalculator.AutoDecision.AutoApproval {
	using Ezbob.Utils.Lingvo;

	public class Result {
		#region constructor

		public Result(int nApprovedAmount, int nOfferLength, bool bIsEmailSendingBanned) {
			ApprovedAmount = nApprovedAmount;
			OfferLength = nOfferLength;
			IsEmailSendingBanned = bIsEmailSendingBanned;
		} // constructor

		#endregion constructor

		#region method ToString

		public override string ToString() {
			return string.Format(
				"amount {0} for {1}, email banned: {2}",
				ApprovedAmount,
				Grammar.Number(OfferLength, "day"),
				IsEmailSendingBanned ? "yes" : "no"
			);
		} // ToString

		#endregion method ToString

		public int ApprovedAmount { get; private set; }
		public int OfferLength { get; private set; }
		public bool IsEmailSendingBanned { get; private set; }
	} // class Result
} // namespace
