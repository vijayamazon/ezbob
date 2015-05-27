namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_PacnetTransactions {
		[PK(true)]
        [DataMember]
        public int PacnetTransactionID { get; set; }

        [FK("NL_FundTransfers", "FundTransferID")]
        [DataMember]
        public int FundTransferID { get; set; }

        [DataMember]
        public DateTime TransactionTime { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }

        [FK("NL_PacnetTransactionStatuses", "PacnetTransactionStatusID")]
        [DataMember]
        public int PacnetTransactionStatusID { get; set; }
        
        [DataMember]
        public DateTime StatusUpdatedTime { get; set; }

		[Length(100)]
		[DataMember]
		public string TrackingNumber { get; set; }
		

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(NL_PacnetTransactions);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(":").Append(prop.GetValue(this)).Append(@"; \n");
			}
			return sb.ToString();
		}
        
    }//class NL_PacnetTransactions
}//ns
