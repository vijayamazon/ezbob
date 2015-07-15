namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_Offers {
		[PK(true)]
		public int OfferID { get; set; }

		[FK("NL_Decisions", "DecisionID")]
		[DataMember]
		public int DecisionID { get; set; }

		[FK("LoanType", "Id")]
		[DataMember]
		public int LoanTypeID { get; set; }

		[FK("NL_RepaymentIntervalTypes", "RepaymentIntervalTypeID")]
		[DataMember]
		public int RepaymentIntervalTypeID { get; set; }

		[DataMember]
		public DateTime StartTime { get; set; }

		[DataMember]
		public DateTime EndTime { get; set; }

		[DataMember]
		public DateTime CreatedTime { get; set; }

		[FK("LoanSource", "LoanSourceID")]
		[DataMember]
		public int LoanSourceID { get; set; }

		[DataMember]
		public int RepaymentCount { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public decimal MonthlyInterestRate { get; set; }



		[DataMember]
		public decimal? SetupFeePercent { get; set; }

		[DataMember]
		public bool SetupFeeAddedToLoan { get; set; }



		[DataMember]
		public decimal? ServicingFeePercent { get; set; }

		[DataMember]
		public decimal? BrokerSetupFeePercent { get; set; }



		[DataMember]
		public int? InterestOnlyRepaymentCount { get; set; }

		[FK("NL_DiscountPlans", "DiscountPlanID")]
		[DataMember]
		public int? DiscountPlanID { get; set; }

		[DataMember]
		public bool IsLoanTypeSelectionAllowed { get; set; }

		[DataMember]
		public bool EmailSendingBanned { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(NL_Offers);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
		}
	}//class NL_Offers
}//ns
