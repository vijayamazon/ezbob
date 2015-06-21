﻿namespace Ezbob.Backend.Models.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using System.Text;
	using DbConstants;

	[DataContract]
	public class ReschedulingArgument {
	
		[DataMember]
		public int LoanID { get; set; }  // loan ID to re-schedule

		[DataMember]
		public string LoanType { get; set; }  // loan old/new

		//[DataMember]
		public DateTime ReschedulingDate { get; set; }  // rescheduling date

		[DataMember]
		public RepaymentIntervalTypes ReschedulingRepaymentIntervalType { get; set; }  // repayment interval type - selected

		//[DataMember]
		public decimal ReschedulingBalance { get; set; } // outstanding balance for rescheduling

		[DataMember]
		public DateTime LoanCloseDate { get; set; } // loan "maturity date", i.e. planned close date

		[DataMember]
		public decimal? PaymentPerInterval { get; set; } // monthly/weekly A amount for "outside" rescheduling

		[DataMember]
		public bool SaveToDB { get; set; } // save result (new schedules) or not

		[DataMember]
		public bool RescheduleIn { get; set; } // type of re-scheduling: for "IN" - true, for "OUT" - false

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(ReschedulingArgument);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
		}

	}
}