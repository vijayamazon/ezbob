namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_Decisions {
        [PK]
        [NonTraversable]
        [DataMember]
        public int DecisionID { get; set; }

        [FK("NL_CashRequests", "CashRequestID")]
        [DataMember]
        public int CashRequestID { get; set; }

        [FK("Security_User", "UserId")]
        [DataMember]
        public int UserID { get; set; }

        [FK("Decisions", "DecisionID")]
        [DataMember]
        public int DecisionNameID { get; set; }

        [DataMember]
        public DateTime DecisionTime { get; set; }

        [DataMember]
        public int Position { get; set; }
        
        [Length("MAX")]
        [DataMember]
        public string Notes { get; set; }

        [DataMember]
        public bool? IsRepaymentPeriodSelectionAllowed { get; set; }

        [DataMember]
        public bool? IsAmountSelectionAllowed { get; set; }

        [DataMember]
        public int? InterestOnlyRepaymentCount { get; set; }

        [DataMember]
        public bool? SendEmailNotification { get; set; }
    }
}

