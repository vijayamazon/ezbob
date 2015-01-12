namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;

	//todo use auto generated object from sales force
	[DataContract]
	public class OpportunityModel {
		[DataMember]
		public string Email { get; set; }
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
		public int? Stage { get; set; }
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
		New, 
		Partial,
		Resell,
		FinishLoan
	}

	//todo define stages
	public enum OpportunityStage {
		s5 = 5,
		s10 = 10,
		s20 = 20,
		s40 = 40,
		s50 = 50,
		s75 = 75,
		s90 = 90
	}
}
