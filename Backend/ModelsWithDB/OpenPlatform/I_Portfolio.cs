namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class I_Portfolio {
		[PK(true)]
		[DataMember]
		public int PortfolioID { get; set; }

		[FK("I_Investor", "InvestorID")]
		[DataMember]
		public int InvestorID { get; set; }

		[FK("I_ProductType", "ProductTypeID")]
		[DataMember]
		public int? ProductTypeID { get; set; }

		[FK("Loan", "Id")]
		[DataMember]
		public int LoanID { get; set; }

		[DataMember]
		public decimal LoanPercentage { get; set; }

		[DataMember]
		public int InitialTerm { get; set; }

		[FK("I_Grade", "GradeID")]
		[DataMember]
		public int? GradeID { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }

		[DataMember]
		public long? NLLoanID { get; set; }
	}//class I_Portfolio
}//ns