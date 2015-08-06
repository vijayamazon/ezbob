namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanFees {
		[PK(true)]
        [DataMember]
		public long LoanFeeID { get; set; }
       
        [FK("NL_Loans", "LoanID")]
        [DataMember]
		public long LoanID { get; set; }

		[FK("NL_LoanFeeTypes", "LoanFeeTypeID")]
		[DataMember]
		public int LoanFeeTypeID { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int? AssignedByUserID { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public DateTime CreatedTime { get; set; }

        [DataMember]
        public DateTime AssignTime { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int? DeletedByUserID { get; set; }

        [DataMember]
        public DateTime? DisabledTime { get; set; }

        [Length(LengthType.MAX)]
        [DataMember]
        public string Notes { get; set; }

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name +  ": ");
			Type t = typeof(NL_LoanFees);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
		}

    }//class NL_LoanFees
}//ns