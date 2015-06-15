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
		public Type LKind { get; set; }  // loan old/new

		[DataMember]
		public decimal ReschedulingBalance { get; set; } // outstanding balance for rescheduling

		[DataMember]
		public RepaymentIntervalTypes ReschedulingRepaymentIntervalType { get; set; }  // repayment interval type 

		[DataMember] // for info only
        public decimal? LoanInterestRate { get; set; } // original loans' Interest Rate

		[DataMember]
		public int IntervalsNum { get; set; }  // calculated repayment intervals number 

		[DataMember]
		public DateTime LoanCloseDate { get; set; } // loan "maturity date", i.e. planned close date

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