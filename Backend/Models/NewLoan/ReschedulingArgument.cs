namespace Ezbob.Backend.Models.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using DbConstants;

	[DataContract]
	public class ReschedulingArgument {

		[DataMember]
		public int LoanID { get; set; }  // loan ID to re-schedule

		[DataMember]
		public DateTime ReschedulingDate { get; set; }  // rescheduling date

		[DataMember]
		public DateTime LoanCloseDate { get; set; } // loan "maturity date", i.e. planned close date

		[DataMember]
		public RepaymentIntervalTypes RepaymentIntervalType { get; set; }  // repayment interval type

		[DataMember]
		public decimal? ReschedulingBalance { get; set; } // outstanding balance for rescheduling

		[DataMember]
		public decimal? PaymentPerInterval { get; set; } // monthly/weekly A amount for "outside" rescheduling

		[DataMember]
		public bool SaveToDB { get; set; } // save result (new schedules) or not

	}
}
