namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanHistory : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanHistoryID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? UserID { get; set; }

		[FK("NL_LoanLegals", "LoanLegalID")]
		[DataMember]
		public long? LoanLegalID { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[FK("NL_RepaymentIntervalTypes", "RepaymentIntervalTypeID")]
		[DataMember]
		public int RepaymentIntervalTypeID { get; set; }

		[DataMember]
		public int RepaymentCount { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }

		[DataMember]
		public DateTime EventTime { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Description { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string AgreementModel { get; set; }

		protected override bool DisplayFieldInToString(string fieldName) {
			return fieldName != "AgreementModel";
		} // DisplayFieldInToString
	} // class NL_LoanHistory
} // ns
