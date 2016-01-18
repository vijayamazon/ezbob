namespace Ezbob.Backend.ModelsWithDB {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;
	
	[DataContract(IsReference = true)]
	public class DecisionHistoryDBModel {
		[DataMember]
		public int DecisionHistoryID { get; set; }

		[Length(50), DataMember]
		public string Action { get; set; }

		[DataMember]
		public DateTime Date { get; set; }

		[Length(250), DataMember]
		public string UnderwriterName { get; set; }

		[Length(250), DataMember]
		public string LoanType { get; set; }

		[Length(512), DataMember]
		public string DiscountPlan { get; set; }

		[Length(50), DataMember]
		public string LoanSourceName { get; set; }

		[DataMember]
		public int RepaymentPeriod { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }

		[DataMember]
		public int ApprovedSum { get; set; }

		[DataMember]
		public int IsLoanTypeSelectionAllowed { get; set; }

		[Length(30), DataMember]
		public string Originator { get; set; }

		[DataMember]
		public decimal ManualSetupFeePercent { get; set; }

		[DataMember]
		public decimal BrokerSetupFeePercent { get; set; }

		[Length(4000), DataMember]
		public string Comment { get; set; }

		[Length(255), DataMember]
		public string Product { get; set; }

		[Length(255), DataMember]
		public string ProductType { get; set; }

		[Length(50), DataMember]
		public string FundingType { get; set; }

		[DataMember]
		public long CashRequestID { get; set; }

		[Length(50), DataMember]
		public string UnderwriterDecision { get; set; }

		[DataMember]
		public DateTime? OfferStart { get; set; }

		[DataMember]
		public DateTime? OfferValidUntil { get; set; }

		[DataMember]
		public int ApprovedRepaymentPeriod { get; set; }
	}//DecisionHistoryDBModel
}//ns
