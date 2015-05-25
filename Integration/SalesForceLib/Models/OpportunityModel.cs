namespace SalesForceLib.Models {
	using System;
	using System.ComponentModel;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class OpportunityModel {
		[DataMember]
		public string Email { get; set; } // account unique identifier
		//----------------------------------------//
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public DateTime? CreateDate { get; set; }
		[DataMember]
		public DateTime? ExpectedEndDate { get; set; }
		[DataMember]
		public DateTime? CloseDate { get; set; }
		[DataMember]
		public int? RequestedAmount { get; set; }
		[DataMember]
		public int? ApprovedAmount { get; set; }
		[DataMember]
		public int? TookAmount { get; set; }
		[DataMember]
		public string Type { get; set; } //OpportunityType enum
		[DataMember]
		public string Stage { get; set; } //OpportunityStage enum
		[DataMember]
		public string DealCloseType { get; set; }
		[DataMember]
		public string DealLostReason { get; set; } // OpportunityDealCloseReason enum
	}

	public enum OpportunityDealCloseReason {
		Won, 
		Lost
	}
	public enum OpportunityType {
		[Description("New Deal")] //created when lead converted to account
		New, 
		[Description("Partial")] // created when took amount is less then approved amount
		Partial,
		[Description("Resell")] // created when customer requests cash
		Resell,
		[Description("Finish Loan")] //created when customer finishes to repay his loan
		FinishLoan,
		[Description("Paid 50% of the balance")]
		FiftyPercentRepaid
	}

	public enum OpportunityStage {
		[Description("New")]
		s5 = 5,
		[Description("Escalated")]
		s20 = 20,
		[Description("Waiting for decision")]
		s40 = 40,
		[Description("Pending information")]
		s50 = 50,
		[Description("Pending Signatures")]
		s75 = 75,
		[Description("Approved")]
		s90 = 90
	}
}
