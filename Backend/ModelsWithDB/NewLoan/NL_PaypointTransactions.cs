namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_PaypointTransactions {
		[PK(true)]
        [DataMember]
        public int PaypointTransactionID { get; set; }

        [FK("NL_Payments", "PaymentID")]
        [DataMember]
        public int PaymentID { get; set; }

        [DataMember]
        public DateTime TransactionTime { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }

        [FK("NL_PaypointTransactionStatuses", "PaypointTransactionStatusID")]
        [DataMember]
        public int PaypointTransactionStatusID { get; set; }

        [Length(100)]
        [DataMember]
        public string PaypointUniqID { get; set; }

        [FK("PayPointCard", "Id")]
        [DataMember]
        public int PaypointCardID { get; set; }

        [Length(10)]
        [DataMember]
        public string IP { get; set; }

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(NL_PaypointTransactions);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
		}

    }//class NL_PaypointTransactions
}//ns
