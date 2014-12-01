namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions {
	using Ezbob.Backend.Models;

	public class AutoDecisionRejectionResponse {
		public bool DecidedToReject { get; set; }
		public bool IsReRejected { get; set; }
		public string AutoRejectReason { get; set; }
		public string CreditResult { get; set; }
		public string UserStatus { get; set; }
		public string SystemDecision { get; set; }
		public RejectionModel RejectionModel { get; set; }
		public string DecisionName { get; set; }
	}
}
