﻿namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_CashRequests {
        [PK]
        [NonTraversable]
        [DataMember]
        public int CashRequestID { get; set; }
        
        [FK("Customer", "Id")]
        [DataMember]
        public int CustomerID { get; set; }
        
        [DataMember]
        public DateTime RequestTime { get; set; }

        [FK("NL_CashRequestOrigins", "CashRequestOriginID")]
        [DataMember]
        public int CashRequestOriginID { get; set; }
        
        [FK("Security_User", "UserId")]
        [DataMember]
        public int UserID { get; set; }

        [FK("CashRequests", "Id")]
        [DataMember]
        public long OldCashRequestID { get; set; }
    }
}
