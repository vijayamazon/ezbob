namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_Decisions {
        [PK(true)]
        [DataMember]
        public int DecisionID { get; set; }

        [FK("NL_CashRequests", "CashRequestID")]
        [DataMember]
        public int CashRequestID { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int UserID { get; set; }

        [FK("Decisions", "DecisionID")]
        [DataMember]
        public int DecisionNameID { get; set; }

        [DataMember]
        public DateTime DecisionTime { get; set; }

        [DataMember]
        public int Position { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }  

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(NL_Decisions);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
		}

    }//class NL_Decisions
}//ns