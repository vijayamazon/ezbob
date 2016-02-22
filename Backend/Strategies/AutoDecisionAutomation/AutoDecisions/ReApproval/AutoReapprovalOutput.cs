namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval {
	using System;
	using Ezbob.Backend.Extensions;

	public class AutoReapprovalOutput {
		public int ApprovedAmount { get; set; }
		public DateTime? AppValidFor { get; set; }
		public long LastApprovedCashRequestID { get; set; }
		public bool IsEmailSendingBanned { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"{0} {1}, CR {2} {3} emails",
				ApprovedAmount.ToString("C0", Strategies.Library.Instance.Culture),
				AppValidFor == null ? "forever" : "until " + AppValidFor.MomentStr(),
				LastApprovedCashRequestID,
				IsEmailSendingBanned ? "without" : "with"
			);
		} // ToString
	} // class AutoReapprovalOutput

	public static class AutoReapprovalOutputExt {
		public static bool IsValid(this AutoReapprovalOutput output) {
			if (output == null)
				return false;

			if (output.AppValidFor == null)
				return false;

			if (output.AppValidFor.Value <= DateTime.UtcNow)
				return false;

			if (output.ApprovedAmount <= 0)
				return false;

			if (output.LastApprovedCashRequestID <= 0)
				return false;

			return true;
		} // IsValid

		public static string Stringify(this AutoReapprovalOutput output) {
			return output == null ? "null" : output.ToString();
		} // Stringify
	} // class AutoReapprovalOutputExt
} // namespace
