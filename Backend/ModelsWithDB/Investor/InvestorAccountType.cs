namespace Ezbob.Backend.ModelsWithDB.Investor {
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class InvestorAccountType
    {
		[PK(true)]
		[DataMember]
        public int ID { get; set; }

		[DataMember]
        public int Name { get; set; }

    }//class InvestorAccountType
}//ns