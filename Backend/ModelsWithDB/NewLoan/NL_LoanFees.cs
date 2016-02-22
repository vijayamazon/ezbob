namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using DbConstants;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanFees : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanFeeID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		[ExcludeFromToString]
		public long LoanID { get; set; }

		[FK("NL_LoanFeeTypes", "LoanFeeTypeID")]
		[DataMember]
		[EnumName(typeof(NLFeeTypes))]
		public int LoanFeeTypeID { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int AssignedByUserID { get; set; } // Use 1 for automatically assigned fees. 

		[DataMember]
		[DecimalFormat("C2")]
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

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? UpdatedByUserID { get; set; }  

		[DataMember]
		public DateTime? UpdateTime { get; set; }

		[FK("LoanCharges", "Id")]
		[DataMember]
		public int? OldFeeID { get; set; }

		[DataMember]
		[NonTraversable]
		public decimal PaidAmount { get; set; }

	} // class NL_LoanFees
} // ns
