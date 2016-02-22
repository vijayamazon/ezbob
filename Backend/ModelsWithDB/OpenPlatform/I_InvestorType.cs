namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.ComponentModel;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_InvestorType {
		[PK]
		[DataMember]
		public int InvestorTypeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }
	}//class InvestorType


	public enum I_InvestorTypeEnum {
		Institutional = 1,
		Private = 2,
		[Description("Hedge Fund")]
		HedgeFund = 3,
	}//enum I_InvestorTypeEnum
}//ns
