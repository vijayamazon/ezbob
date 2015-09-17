namespace Ezbob.Backend.ModelsWithDB.Investor {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class InvestorOverallStatistics
    {
		[PK(true)]
		[DataMember]
        public int ID { get; set; }

        [DataMember]
        public int InvestorID { get; set; }

		[DataMember]
        public String TotalYield { get; set; }

        [DataMember]
        public String TotalAccumulatedRepayments { get; set; }

        [DataMember]
        public String Defaults { get; set; }

        [DataMember]
        public String Recoveries { get; set; }

        [DataMember]
        public TimeSpan TimeSpan { get; set; }

    }//class InvestorOverallStatistics
}//ns


