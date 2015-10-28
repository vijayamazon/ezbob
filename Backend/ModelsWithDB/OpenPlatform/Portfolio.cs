namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
    public class Portfolio
    {
		[PK(true)]
		[DataMember]
        public int ID { get; set; }

        [DataMember]
        public int InvestorID { get; set; }

		[DataMember]
        public String LoanID { get; set; }

        [DataMember]
        public int LoanPercentage { get; set; }

        [DataMember]
        public String InitialTerm { get; set; }

        [DataMember]
        public int GradeId { get; set; }

        [DataMember]
        public TimeSpan TimeSpan { get; set; }

    }//class Portfolio
}//ns