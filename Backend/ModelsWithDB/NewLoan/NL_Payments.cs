namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_Payments {
		[PK(true)]
        [DataMember]
        public int PaymentID { get; set; }

        [FK("LoanTransactionMethod", "Id")]
        [DataMember]
        public int PaymentMethodID { get; set; }

        [DataMember]
        public DateTime PaymentTime { get; set; }

		[DataMember]
		public decimal? Amount { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public DateTime CreationTime { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int? CreatedByUserID { get; set; }

        [DataMember]
        public DateTime? DeletionTime { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int? DeletedByUserID { get; set; }

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
			Type t = typeof(NL_Payments);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
	    }
    }//class NL_Payments
}//ns
