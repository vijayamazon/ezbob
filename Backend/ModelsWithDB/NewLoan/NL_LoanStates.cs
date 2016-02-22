namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanStates : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanStateID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		[DataMember]
		public DateTime InsertDate { get; set; }

		[DataMember]
		public int NumberOfPayments { get; set; }

		[DataMember]
		public decimal OutstandingPrincipal { get; set; }

		[DataMember]
		public decimal OutstandingInterest { get; set; }

		[DataMember]
		public decimal OutstandingFee { get; set; }

		[DataMember]
		public decimal PaidPrincipal { get; set; }

		[DataMember]
		public decimal PaidInterest { get; set; }

		[DataMember]
		public decimal PaidFee { get; set; }

		[DataMember]
		public int LateDays { get; set; }

		[DataMember]
		public decimal LatePrincipal { get; set; }

		[DataMember]
		public decimal LateInterest { get; set; }

		[DataMember]
		public decimal WrittenOffPrincipal { get; set; }

		[DataMember]
		public decimal WrittenOffInterest { get; set; }

		[DataMember]
		public decimal WrittentOffFees { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }
	} // class NL_LoanStates
} // ns
