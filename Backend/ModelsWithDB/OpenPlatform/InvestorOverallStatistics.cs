namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class InvestorOverallStatistics {
        [PK(true)]
        [DataMember]
		public int InvestorOverallStatisticsID { get; set; }

		[FK("Investor", "InvestorID")]
        [DataMember]
		public int InvestorID { get; set; }

		[FK("InvestorAccountType", "InvestorAccountTypeID")]
		[DataMember]
		public int InvestorAccountTypeID { get; set; }

		[DataMember]
		public decimal? TotalYield { get; set; }

		[DataMember]
		public decimal? TotalAccumulatedRepayments { get; set; }

		[DataMember]
		public decimal? Defaults { get; set; }

		[DataMember]
		public decimal? Recoveries { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class InvestorOverallStatistics
}//ns
