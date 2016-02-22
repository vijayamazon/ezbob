namespace Ezbob.Backend.Models.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using System.Text;
	using DbConstants;

	[DataContract]
	public class ReschedulingArgument {

		[DataMember]
		public int LoanID { get; set; }  // "old" loan ID to re-schedule

		[DataMember]
		public string LoanType { get; set; }  // loan old/new

		[DataMember]
		public DateTime ReschedulingDate { get; set; }  // rescheduling date

		[DataMember]
		public RepaymentIntervalTypes ReschedulingRepaymentIntervalType { get; set; }  // repayment interval type - selected

		[DataMember]
		public decimal? PaymentPerInterval { get; set; } // monthly/weekly A amount for "outside" rescheduling

		[DataMember]
		public bool SaveToDB { get; set; } // save result (new schedules) or not

		[DataMember]
		public bool RescheduleIn { get; set; } // type of re-scheduling: for "IN" - true, for "OUT" - false

		[DataMember]
		public bool StopFutureInterest { get; set; } // freeze interest for "outside", starts from ReschedulingDate
		
		//[DataMember]
		//public long NLLoanID { get; set; }  // nl loan ID to re-schedule

		[DataMember]
		public int? UserID { get; set; } 
	
		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(ReschedulingArgument);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("\n");
			}
			return sb.ToString();
		}

	}
}