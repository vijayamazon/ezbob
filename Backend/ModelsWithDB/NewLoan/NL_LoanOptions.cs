namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_LoanOptions {
        [PK]
        [NonTraversable]
        [DataMember]
        public int LoanOptionsID { get; set; }

    }//class NL_LoanOptions
}//ns
