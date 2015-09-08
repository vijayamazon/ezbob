namespace Ezbob.Backend.Models.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using DbConstants;

	[DataContract]
	public class ReschedulingResult /*: AStringable*/ {
		[DataMember]
		public long LoanID { get; set; } // loan ID to re-schedule

		[DataMember]
		public decimal ReschedulingBalance { get; set; } // outstanding balance for rescheduling

		[DataMember]
		public RepaymentIntervalTypes ReschedulingRepaymentIntervalType { get; set; }  // repayment interval type 

		[DataMember] // for info only
		public decimal LoanInterestRate { get; set; } // original loans' Interest Rate

		[DataMember]
		public int IntervalsNum { get; set; } // calculated repayment intervals number in months

		[DataMember]
		public decimal? FirstPaymentInterest { get; set; } // calculated interest to be paid in the first new payment 

		[DataMember]
		public DateTime LoanCloseDate { get; set; } // loan "maturity date", i.e. planning close date

		[DataMember]
		public decimal DefaultPaymentPerInterval { get; set; } // default "payment per interval" (if args.PaymentPerInterval == 0, fill DefaultPaymentPerInterval)

		[DataMember]
		public string Error { get; set; }
	} // class ReschedulingResult
} // namespace
