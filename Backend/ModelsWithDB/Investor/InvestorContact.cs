namespace Ezbob.Backend.ModelsWithDB.Investor {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class InvestorContact
    {
		[PK(true)]
		[DataMember]
        public int ID { get; set; }

        [DataMember]
        public int  InvestorID { get; set; }

		[DataMember]
        public String FirstName { get; set; }

        [DataMember]
        public String LastName { get; set; }

        [DataMember]
        public String Email { get; set; }

        [DataMember]
        public int Role { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public String Comment { get; set; }

        [DataMember]
        public bool IsPrimary { get; set; }

        [DataMember]
        public int MobilePhone { get; set; }

        [DataMember]
        public int OfficePhone { get; set; }

        [DataMember]
        public TimeSpan TimeSpan { get; set; }

    }//class Investor
}//ns