namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_InvestorFundsAllocation {
        [PK(true)]
        [DataMember]
		public int InvestorFundsAllocationID { get; set; }

		[FK("I_InvestorBankAccount", "InvestorBankAccountID")]
        [DataMember]
        public int InvestorBankAccountID { get; set; }

		[DataMember]
		public decimal? Amount { get; set; }

		[DataMember]
		public DateTime? AllocationTimestamp { get; set; }

		[DataMember]
		public DateTime? ReleaseTimestamp { get; set; }
	}//class I_InvestorFundsAllocation
}//ns
