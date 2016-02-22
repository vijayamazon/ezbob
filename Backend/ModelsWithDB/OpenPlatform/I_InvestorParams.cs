namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class I_InvestorParams {
        [PK(true)]
        [DataMember]
		public int InvestorParamsID { get; set; }

		[FK("I_Investor", "InvestorID")]
        [DataMember]
        public int? InvestorID { get; set; }

		[FK("I_Parameter", "ParameterID")]
		[DataMember]
		public int ParameterID { get; set; }

		[DataMember]
		public decimal Value { get; set; }
		
		[DataMember]
		public int Type { get; set; }
        
		[DataMember]
		public bool AllowedForConfig { get; set; }

		[NonTraversable]
		[DataMember]
		public RuleType TypeEnum { get { return (RuleType)Type; } }
	}//class I_UWInvestorConfigurationParam
}//ns
