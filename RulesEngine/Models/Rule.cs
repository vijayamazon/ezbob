namespace RulesEngine.Models {
    using System.Runtime.Serialization;
    using Ezbob.Backend.ModelsWithDB.Investor;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class Rule
    {
		[PK(true)]
        [DataMember]
        public int RuleID { get; set; }

        [DataMember]
        public int UserID { get; set; }

        [DataMember]
        public RuleType RuleType { get; set; }

        [DataMember]
        public int InvestorID { get; set; }

		[DataMember]
        public string MemberNameSource { get; set; }

        [DataMember]
        public string MemberNameTarget { get; set; }

        [DataMember]
        public int LeftParamID { get; set; }

		[DataMember]
        public int RightParamID { get; set; }

        [DataMember]
        public Operator Operator { get; set; }

        [DataMember]
        public bool IsRoot { get; set; }

    }//class Rule
}//ns
