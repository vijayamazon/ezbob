namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System.Collections.Generic;
	using Ezbob.Backend.Models;

	public class AutoDecisionRejectionResponse
	{
		public AutoDecisionRejectionResponse()
		{
			RejectionConditions = new List<AutoDecisionCondition>();
		}

		public bool DecidedToReject { get; set; }
		public bool IsReRejected { get; set; }
		public string AutoRejectReason { get; set; }
		public string CreditResult { get; set; }
		public string UserStatus { get; set; }
		public string SystemDecision { get; set; }
		public RejectionModel RejectionModel { get; set; }
		public List<AutoDecisionCondition> RejectionConditions { get; set; }
		public string DecisionName { get; set; }
	}
}
