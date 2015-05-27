namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_FundTransfers {
		[PK(true)]
        [DataMember]
        public int FundTransferID { get; set; }

        [FK("NL_Loans", "LoanID")]
        [DataMember]
        public int LoanID { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public DateTime TransferTime { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

		[FK("LoanTransactionMethod", "Id")]
		[DataMember]
		public int LoanTransactionMethodID { get; set; }

	
		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType()+ ": ");
			Type t = typeof(NL_FundTransfers);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(":").Append(prop.GetValue(this)).Append(@"; \n");
			}
			return sb.ToString();
		}

    }//class NL_FundTransfers
}//ns