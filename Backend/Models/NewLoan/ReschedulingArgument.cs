namespace Ezbob.Backend.Models.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using DbConstants;

	[DataContract]
	public class ReschedulingArgument /*: AStringable*/ {
		[DataMember]
		public long LoanID { get; set; }  // loan ID to re-schedule

		[DataMember]
		public string LoanType { get; set; }  // loan old/new

		[DataMember]
		public DateTime ReschedulingDate { get; set; }  // rescheduling date

		[DataMember]
		public RepaymentIntervalTypes ReschedulingRepaymentIntervalType { get; set; }  // repayment interval type - selected
	
		[DataMember]
		public decimal? PaymentPerInterval { get; set; } // monthly/weekly  amount for "outside" rescheduling

		[DataMember]
		public bool SaveToDB { get; set; } // save result (new schedules) or not

		[DataMember]
		public bool RescheduleIn { get; set; } // type of re-scheduling: for "IN" - true, for "OUT" - false
	} // class ReschedulingArgument
		[DataMember]
		public bool StopFutureInterest { get; set; } // freeze interest for "outside", starts from ReschedulingDate

					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("\n");