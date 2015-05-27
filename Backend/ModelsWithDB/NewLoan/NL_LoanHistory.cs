namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanHistory {
		[PK(true)]
		[DataMember]
		public int LoanHistoryID { get; set; }

		
		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public int? LoanID { get; set; }


			[FK("Security_User", "UserId")]
		[DataMember]
		public int? UserID { get; set; }


		[FK("NL_LoanLegals", "LoanLegalID")]
		[DataMember]
		public int? LoanLegalID { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public int RepaymentCount { get; set; }

		[DataMember] 
		public decimal? InterestRate { get; set; }

		[DataMember]
		public DateTime EventTime { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Description { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string AgreementModel { get; set; }


		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().FullName + @": \n");
			Type t = typeof(NL_LoanHistory);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null) {
					if (prop.Name != "AgreementModel") {
					//	Console.WriteLine(prop.GetValue(this));
					//} else {
						sb.Append(prop.Name)
							.Append(@":" + prop.GetValue(this))
							.Append(@"; \n");
					}
				}
			}
			return sb.ToString();
		}
	}//class NL_LoanHistory
}//ns
