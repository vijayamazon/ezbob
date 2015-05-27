namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using System.Text;

	[DataContract]
	public class OfferForLoan {

		[DataMember]
		public int LoanLegalID { get; set; }
		[DataMember]
		public decimal LoanLegalAmount { get; set; }
		[DataMember]
		public int LoanLegalRepaymentPeriod { get; set; }
		[DataMember]
		public int OfferID { get; set; }
		[DataMember]
		public int LoanTypeID { get; set; }
		[DataMember]
		public int RepaymentIntervalTypeID { get; set; }
		[DataMember]
		public int LoanSourceID { get; set; }
		[DataMember]
		public int OfferRepaymentCount { get; set; }
		[DataMember]
		public decimal OfferAmount { get; set; }
		[DataMember]
		public decimal MonthlyInterestRate { get; set; }
		[DataMember]
		public decimal SetupFeePercent { get; set; }
		[DataMember]
		public decimal BrokerSetupFeePercent { get; set; }
		[DataMember]
		public int InterestOnlyRepaymentCount { get; set; }
		[DataMember]
		public string DiscountPlan { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + @": \n");
			Type t = typeof(OfferForLoan);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(":").Append(prop.GetValue(this)).Append(@"; \n<br/>");
			}
			return sb.ToString();
		}
	}
}
