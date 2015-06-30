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
		public RepaymentIntervalTypes ReschedulingRepaymentIntervalType { get; set; }  // repayment interval type 

		[DataMember] // for info only
		public decimal LoanInterestRate { get; set; } // original loans' Interest Rate

		[DataMember]
		public int IntervalsNum { get; set; } // calculated repayment intervals number in months

		[DataMember]
		public int IntervalsNumWeeks { get; set; }  // calculated repayment intervals number in weeks

		[DataMember]
		public DateTime LoanCloseDate { get; set; } // loan "maturity date", i.e. planning close date

		[DataMember]
		public decimal DefaultPaymentPerInterval { get; set; } // interest accrued untill ReschedulingDate including

		public decimal? Fees { get; set; } // fees in loan

		[DataMember]
		public string Error { get; set; }

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(ReschedulingResult);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
		}

	} //ReschedulingResult
}