namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanLienLinks : AStringable {
		[PK(true)]
		[DataMember]
		public int LoanLienLinkID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		[FK("LoanLien", "Id")]
		[DataMember]
		public int LoanLienID { get; set; }

		[DataMember]
		public decimal Amount { get; set; }
	} // class NL_LoanLienLinks
} // ns
