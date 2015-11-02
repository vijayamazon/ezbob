namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Text;
	using DbConstants;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_Payments : AStringable {
		[PK(true)]
		[DataMember]
		public long PaymentID { get; set; }

		[FK("LoanTransactionMethod", "Id")]
		[DataMember]
		[EnumName(typeof(NLLoanTransactionMethods))]
		public int PaymentMethodID { get; set; }

		[DataMember]
		public DateTime PaymentTime { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[FK("NL_PaymentStatuses", "PaymentStatusID")]
		[DataMember]
		[EnumName(typeof(NLPaymentStatuses))]
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
		private List<NL_LoanSchedulePayments> _schedulePayments = new List<NL_LoanSchedulePayments>();
		private List<NL_LoanFeePayments> _feePayments = new List<NL_LoanFeePayments>();


		[DataMember]
		[NonTraversable]
		[ExcludeFromToString]
		public List<NL_PaypointTransactions> PaypointTransactions {
			get { return this._paypointTransactions; }
			set { this._paypointTransactions = value; }
		}

		[DataMember]
		[NonTraversable]
		[ExcludeFromToString]
		public List<NL_LoanSchedulePayments> SchedulePayments {
			get { return this._schedulePayments; }
			set { this._schedulePayments = value; }
		}

		[DataMember]
		[NonTraversable]
		[ExcludeFromToString]
		public List<NL_LoanFeePayments> FeePayments {
			get { return this._feePayments; }
			set { this._feePayments = value; }
		}

		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public override string ToString() {
			// payment
			StringBuilder sb = new StringBuilder().Append(Environment.NewLine).Append(PrintHeadersLine(typeof(NL_Payments))).Append(ToStringAsTable());

			if (PaypointTransactions.Count > 0) {
				sb.Append("Paypoint transactions:").Append(Environment.NewLine).Append(PrintHeadersLine(typeof(NL_PaypointTransactions)));
				PaypointTransactions.ForEach(s => sb.Append(s.ToStringAsTable()));
			} else
				sb.Append("No paypoint transactions.").Append(Environment.NewLine);

			if (FeePayments.Count > 0) {
				sb.Append("Fees payments:").Append(Environment.NewLine).Append(PrintHeadersLine(typeof(NL_LoanFeePayments)));
				FeePayments.ForEach(s => sb.Append(s.ToStringAsTable()));
			} else
				sb.Append("No Fees payments.").Append(Environment.NewLine);
			
			if (SchedulePayments.Count > 0) {
				sb.Append("Schedule payments:").Append(Environment.NewLine).Append(PrintHeadersLine(typeof(NL_LoanSchedulePayments)));
				SchedulePayments.ForEach(s => sb.Append(s.ToStringAsTable()));
			} else
				sb.Append("No Schedule payments.").Append(Environment.NewLine);

			return sb.ToString();
		}

	} // class NL_Payments
} // ns
