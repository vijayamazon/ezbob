namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_BlendedLoans : AStringable {
		[PK(true)]
		[DataMember]
		public long BlendedLoanID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }
	} // class NL_BlendedLoans
} // ns
