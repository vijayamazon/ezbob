namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_InvestorConfigurationParam {
        [PK(true)]
        [DataMember]
		public int InvestorConfigurationParamID { get; set; }

		[FK("I_Investor", "InvestorID")]
        [DataMember]
        public int InvestorID { get; set; }

		[FK("I_Parameter", "ParameterID")]
		[DataMember]
		public int ParameterID { get; set; }
		
		[DataMember]
		public decimal Value { get; set; }
	}//class I_InvestorConfigurationParam
}//ns
