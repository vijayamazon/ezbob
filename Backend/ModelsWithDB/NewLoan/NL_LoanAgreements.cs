namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanAgreements : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanAgreementID { get; set; }

		[FK("NL_LoanHistory", "LoanHistoryID")]
		[DataMember]
		public long LoanHistoryID { get; set; }

		[Length(250)]
		[DataMember]
		public string FilePath { get; set; }

		[FK("LoanAgreementTemplate", "Id")]
		[DataMember]
		public int? LoanAgreementTemplateID { get; set; }
	} // class NL_LoanAgreements
} // ns
