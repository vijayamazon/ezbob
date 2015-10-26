namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Text;
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
		private List<NL_PaypointTransactions> _paypointTransactions = new List<NL_PaypointTransactions>();

		[DataMember]
		[NonTraversable]
		public List<NL_PaypointTransactions> PaypointTransactions {
			get { return this._paypointTransactions; }
			set { this._paypointTransactions = value; }
		}

		/// <summary>
		/// prints data only
		/// to print headers line call base static GetHeadersLine 
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			// payment
			StringBuilder sb = new StringBuilder().Append(ToStringTable())
				.Append(Environment.NewLine);

			sb.Append("Paypoint Transactions:");

			if (PaypointTransactions.Count > 0) {
				sb.Append(GetHeadersLine(typeof(NL_PaypointTransactions))).Append(Environment.NewLine);
				PaypointTransactions.ForEach(s => sb.Append(s.ToString()));
			} // else sb.Append("No paypoint transactions found");

			return sb.ToString();
		}

	} // class NL_Payments
} // ns
