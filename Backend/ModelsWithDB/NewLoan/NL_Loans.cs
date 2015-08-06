namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_Loans {
		[PK(true)]
		[DataMember]
		public long LoanID { get; set; }

		[FK("NL_Offers", "OfferID")]
		[DataMember]
		public long OfferID { get; set; }

		[FK("LoanType", "Id")]
		[DataMember]
		public int LoanTypeID { get; set; }

		[FK("NL_RepaymentIntervalTypes", "RepaymentIntervalTypeID")]
		[DataMember]
		public int RepaymentIntervalTypeID { get; set; }

		[FK("NL_LoanStatuses", "LoanStatusID")]
		[DataMember]
		public int LoanStatusID { get; set; }

		[FK("NL_EzbobBankAccounts", "EzbobBankAccountID")]
		[DataMember]
		public int? EzbobBankAccountID { get; set; }

		[FK("LoanSource", "LoanSourceID")]
		[DataMember]
		public int LoanSourceID { get; set; }

		[DataMember]
		public int Position { get; set; }

		[DataMember]
		public decimal InitialLoanAmount { get; set; }

		[DataMember]
		public DateTime CreationTime { get; set; }

		[DataMember]
		public DateTime IssuedTime { get; set; }

		[DataMember]
		public int RepaymentCount { get; set; }

		[Length(10)]
		[DataMember]
		public string Refnum { get; set; }

		[DataMember]
		public DateTime? DateClosed { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }

		[DataMember]
		public int? InterestOnlyRepaymentCount { get; set; }

		[FK("Loan", "Id")]
		[DataMember]
		public long OldLoanID { get; set; }

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": \n");
			Type t = typeof(NL_Loans);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
		}
	}//class NL_Loans
}//ns