namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_DecisionRejectReasons {
        [PK]
        [NonTraversable]
        [DataMember]
        public int DecisionRejectReasonID { get; set; }

        [FK("NL_Decisions", "DecisionID")]
        [DataMember]
        public int DecisionID { get; set; }

        [FK("RejectReason", "Id")]
        [DataMember]
        public int RejectReasonID { get; set; }
    }//class NL_DecisionRejectReasons
}//ns

