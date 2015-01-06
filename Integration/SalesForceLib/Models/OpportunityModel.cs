namespace SalesForceLib.Models {
	using System;

	//todo use auto generated object from sales force
	public class OpportunityModel {
		public string Email { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime ExpectedEndDate { get; set; }
		public DateTime? CloseDate { get; set; }
		public int? RequestedAmount { get; set; }
		public int? ApprovedAmount { get; set; }
		public int? TookAmount { get; set; }
		public string Type { get; set; } //??? is it needed
		public int Stage { get; set; }
		public string DealCloseType { get; set; }
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
