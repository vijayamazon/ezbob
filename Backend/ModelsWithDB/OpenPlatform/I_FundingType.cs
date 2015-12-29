namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.ComponentModel;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_FundingType {
		[PK]
        [DataMember]
		public int FundingTypeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }

	}//class I_FundingType

	public enum I_FundingTypeEnum {
		[Description("Co investment")]
		CoInvestment = 1,
		[Description("Full investment")]
		FullInvestment = 2,
		[Description("Pooled investment")]
		PooledInvestment = 3,
	}//enum I_FundingTypeEnum
}//ns
