namespace Ezbob.Backend.Models.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using System.Text;
	using DbConstants;

	[DataContract]
	public class ReschedulingResult {
		[DataMember]
		public int LoanID { get; set; }  // loan ID to re-schedule

		[DataMember]
		public decimal ReschedulingBalance { get; set; } // outstanding balance for rescheduling

		[DataMember]
		public decimal OpenPrincipal { get; set; } // open principal for rescheduling

		[DataMember]
		public RepaymentIntervalTypes ReschedulingRepaymentIntervalType { get; set; }  // repayment interval type 

		[DataMember] // for info only
		public decimal LoanInterestRate { get; set; } // original loans' Interest Rate

		[DataMember]
		public int IntervalsNum { get; set; } // calculated repayment intervals number in months

		[DataMember]
		public decimal? FirstPaymentInterest { get; set; } // calculated interest to be paid in the first new payment 

		[DataMember]
		public decimal DefaultPaymentPerInterval { get; set; } // default "payment per interval" (if args.PaymentPerInterval == 0, fill DefaultPaymentPerInterval)		

		[DataMember]
		public DateTime ReschedulingIntervalStart { get; set; } // ReschedulingIntervalStart: for "within"/"outside" - tomorrow

		[DataMember]
		public DateTime? ReschedulingIntervalEnd { get; set; } // ReschedulingIntervalEnd: for "within" - ReschedulingDateStart + 30 days, for "outside" - not relevant

		[DataMember]
		public string Error { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public bool BlockAction { get; set; } // re-scheduling allowed or not

		[DataMember]
		public DateTime LastPaidItemDate { get; set; } // date of last paid or partially paid installment


		private DateTime _firstItemDate;
		// new schedule first payment date
		public DateTime FirstItemDate {
			get { return this._firstItemDate.Date; }
			set { this._firstItemDate = value; }
		}

		public DateTime LoanCloseDate { get; set; } // loan "maturity date", i.e. planned close date

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(ReschedulingResult);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("\n");
			}
			return sb.ToString();
		}

	} //ReschedulingResult
}