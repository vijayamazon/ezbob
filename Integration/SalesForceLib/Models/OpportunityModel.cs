namespace SalesForceLib.Models {
	using System;
	using System.ComponentModel;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class OpportunityModel {
		[DataMember]
		public string Email { get; set; }
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
		public string Type { get; set; } //??? is it needed
		[DataMember]
		public string Stage { get; set; }
		[DataMember]
		public string DealCloseType { get; set; }
		[DataMember]
		public string DealLostReason { get; set; }
	}

	public enum OpportunityDealCloseReason {
		Won, 
		Lost
	}
	public enum OpportunityType {
		[Description("New Deal")]
		New, 
		[Description("Partial")]
		Partial,
		[Description("Resell")]
		Resell,
		[Description("Finish Loan")]
		FinishLoan
	}

	//todo define stages
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
