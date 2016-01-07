namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanRollovers : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanRolloverID { get; set; }

		[FK("NL_LoanHistory", "LoanHistoryID")]
		[DataMember]
		public long LoanHistoryID { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int CreatedByUserID { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? DeletedByUserID { get; set; }

		[FK("NL_LoanFees", "LoanFeeID")]
		[DataMember]
		public long? LoanFeeID { get; set; }

		[DataMember]
		public DateTime CreationTime { get; set; }

		[DataMember]
		public DateTime ExpirationTime { get; set; }

		[DataMember]
		public DateTime? CustomerActionTime { get; set; }

		[DataMember]
		public bool IsAccepted { get; set; }

		[DataMember]
		public DateTime? DeletionTime { get; set; }

		//[DataMember]
		//[NonTraversable]
		//public bool Processed { get; set; }

	} // class NL_LoanRollovers
} // ns
