namespace Ezbob.Backend.ModelsWithDB.Investor
{
    using System;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class Investor 
    {
        [PK(true)]
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public InvestorType InvestorType { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String Email { get; set; }

        [DataMember]
        public String Password { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public TimeSpan TimeSpan { get; set; }

        public InvestorParameters InvestorParameters { get; set; }

 
    }//class Investor
}//ns