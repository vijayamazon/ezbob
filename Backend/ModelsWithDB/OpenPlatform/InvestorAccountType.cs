namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class InvestorAccountType {
        [PK(true)]
        [DataMember]
		public int InvestorAccountTypeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }
	}//class InvestorAccountType
}//ns
