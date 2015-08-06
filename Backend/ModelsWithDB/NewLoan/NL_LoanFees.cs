namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanFees : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanFeeID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		[FK("NL_LoanFeeTypes", "LoanFeeTypeID")]
		[DataMember]
		public int LoanFeeTypeID { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? AssignedByUserID { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public DateTime CreatedTime { get; set; }

		[DataMember]
		public DateTime AssignTime { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? DeletedByUserID { get; set; }

		[DataMember]
		public DateTime? DisabledTime { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }
	} // class NL_LoanFees
} // ns
