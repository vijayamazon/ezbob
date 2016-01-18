namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_InvestorAccountType {
        [PK]
        [DataMember]
		public int InvestorAccountTypeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }
	}//class I_InvestorAccountType

	public enum I_InvestorAccountTypeEnum {
		Funding = 1,
		Repayments = 2,
		Bridging = 3,
	}//enum I_InvestorTypeEnum
}//ns
