namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval {
	using System;

	public class AutoReapprovalOutput {
		public int ApprovedAmount { get; set; }
		public DateTime? AppValidFor { get; set; }
		public long LastApprovedCashRequestID { get; set; }
		public bool IsEmailSendingBanned { get; set; }
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
	} // class AutoReapprovalOutputExt
} // namespace
