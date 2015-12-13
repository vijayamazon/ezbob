namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_UWInvestorConfigurationParam {
        [PK(true)]
        [DataMember]
		public int UWInvestorConfigurationParamID { get; set; }

		[FK("I_Investor", "InvestorID")]
        [DataMember]
        public int InvestorID { get; set; }

		[FK("I_Parameter", "ParameterID")]
		[DataMember]
		public int ParameterID { get; set; }
		
		[DataMember]
		public decimal Value { get; set; }

		[DataMember]
		public bool AllowedForConfig { get; set; }
	}//class I_UWInvestorConfigurationParam
}//ns
