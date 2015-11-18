namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Collections.Generic;

	internal class DecisionToApply {
		public DecisionToApply(int underwriterID, int customerID, long cashRequestID) {
			Customer = new CustomerData(customerID);
			CashRequest = new CashRequestData(underwriterID, cashRequestID);
		} // constructor

		public CustomerData Customer { get; private set; }
		public CashRequestData CashRequest { get; private set; }

		public class CustomerData {
			public CustomerData(int customerID) {
				ID = customerID;
				IsWaitingForSignature = null;
			} // constructor

			public int ID { get; set; }

			public string CreditResult { get; set; }
			public string UnderwriterName { get; set; }

			public bool? IsWaitingForSignature { get; set; }
			public DateTime DateApproved { get; set; }
			public decimal ManagerApprovedSum { get; set; }
			public string ApprovedReason { get; set; }
			public decimal CreditSum { get; set; }
			public int NumApproves { get; set; }
			public int IsLoanTypeSelectionAllowed { get; set; }

			public DateTime DateRejected { get; set; }
			public string RejectedReason { get; set; }
			public int NumRejects { get; set; }
		} // class CustomerData

		public class CashRequestData {
			public CashRequestData(int underwriterID, long cashRequestID) {
				UnderwriterID = underwriterID;
				ID = cashRequestID;
				RejectionReasons = new List<int>();
			} // constructor

			public long ID { get; set; }

			public int UnderwriterID { get; set; }
			public DateTime UnderwriterDecisionDate { get; set; }
			public string UnderwriterDecision { get; set; }
			public string UnderwriterComment { get; set; }

			public int ManagerApprovedSum { get; set; }

			public List<int> RejectionReasons { get; private set; }
		} // class CashRequestData
	} // class DecisionToApply
} // namespace
