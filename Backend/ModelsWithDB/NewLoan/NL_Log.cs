namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    public class NL_Log
    {
        [PK(true)]
        [DataMember]
        public long LogID { get; set; }
        [DataMember]
        public string Sevirity { get; set; }
        [DataMember]
        public int? UserID { get; set; }
        [DataMember]
        public int? CustomerID { get; set; }
        [DataMember]
        public string Args { get; set; }
        [DataMember]
        public string Result { get; set; }
        [DataMember]
        public string Referrer { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]        
        public string Exception { get; set; }
        [DataMember]
        public string Stacktrace { get; set; }
        [DataMember]
        public DateTime TimeStamp { get; set; }
    }
}
