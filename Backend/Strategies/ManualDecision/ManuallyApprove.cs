namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;

	using DecisionActions = EZBob.DatabaseLib.Model.Database.Status;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class ManuallyApprove : AStoredProcedure {
		public ManuallyApprove(DecisionToApply decision, AConnection db, ASafeLog log) : base(db, log) {
			this.decision = decision;
		} // constructor

		public override bool HasValidParameters() {
			return (CustomerID > 0) && (CashRequestID > 0) && (CashRequestManagerApprovedSum > 0);
		} // HasValidParameters

		public int CustomerID { get { return this.decision.Customer.ID; } set { } }
		public string CreditResult { get { return this.decision.Customer.CreditResult; } set { } }
		public string Status { get { return DecisionActions.Approved.ToString(); } set { } }
		public string UnderwriterName { get { return this.decision.Customer.UnderwriterName; } set { } }
		public bool? IsWaitingForSignature { get { return this.decision.Customer.IsWaitingForSignature; } set { } }
		public DateTime DateApproved { get { return this.decision.Customer.DateApproved; } set { } }
		public string ApprovedReason { get { return this.decision.Customer.ApprovedReason; } set { } }
		public decimal CreditSum { get { return this.decision.Customer.CreditSum; } set { } }
		public decimal CustomerManagerApprovedSum { get { return this.decision.Customer.ManagerApprovedSum; } set { } }
		public int NumApproves { get { return this.decision.Customer.NumApproves; } set { } }
		public int IsLoanTypeSelectionAllowed { get { return this.decision.Customer.IsLoanTypeSelectionAllowed; } set { } }

		public long CashRequestID { get { return this.decision.CashRequest.ID; } set { } }
		public int UnderwriterID { get { return this.decision.CashRequest.UnderwriterID; } set { } }
		public DateTime UnderwriterDecisionDate { get { return this.decision.CashRequest.UnderwriterDecisionDate; } set { } }
		public string UnderwriterDecision { get { return this.decision.CashRequest.UnderwriterDecision; } set { } }
		public string UnderwriterComment { get { return this.decision.CashRequest.UnderwriterComment; } set { } }
		public int CashRequestManagerApprovedSum { get { return this.decision.CashRequest.ManagerApprovedSum; } set { } }

		private readonly DecisionToApply decision;
	} // class ManuallyApprove
} // namespace
