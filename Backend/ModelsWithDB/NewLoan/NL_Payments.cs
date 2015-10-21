namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_Payments : AStringable {
		[PK(true)]
		[DataMember]
		public long PaymentID { get; set; }

		[FK("LoanTransactionMethod", "Id")]
		[DataMember]
		public int PaymentMethodID { get; set; }

		[DataMember]
		public DateTime PaymentTime { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[FK("NL_PaymentStatuses", "PaymentStatusID")]
		[DataMember]
		public int PaymentStatusID { get; set; }

		[DataMember]
		public DateTime CreationTime { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int CreatedByUserID { get; set; }

		[DataMember]
		public DateTime? DeletionTime { get; set; }

		[DataMember]
		public DateTime? DeletionNotificationTime { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? DeletedByUserID { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		// additions
		private List<NL_PaypointTransactions> _paypointTransactionses = new List<NL_PaypointTransactions>();

		[DataMember]
		[NonTraversable]
		public List<NL_PaypointTransactions> PaypointTransactionses {
			get { return this._paypointTransactionses; }
			set { this._paypointTransactionses = value; }
		}

		

	} // class NL_Payments
} // ns
