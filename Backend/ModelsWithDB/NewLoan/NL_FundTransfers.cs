﻿namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_FundTransfers {
		[PK(true)]
        [DataMember]
		public long FundTransferID { get; set; }

        [FK("NL_Loans", "LoanID")]
        [DataMember]
		public long LoanID { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public DateTime TransferTime { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

		[FK("LoanTransactionMethod", "Id")]
		[DataMember]
		public int LoanTransactionMethodID { get; set; }

	
		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name+ ": ");
			Type t = typeof(NL_FundTransfers);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(":").Append(prop.GetValue(this)).Append(@"; \n");
			}
			return sb.ToString();
		}

    }//class NL_FundTransfers
}//ns