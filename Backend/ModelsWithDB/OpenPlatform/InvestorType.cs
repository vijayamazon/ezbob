namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class InvestorType {
        [PK(true)]
        [DataMember]
		public int InvestorTypeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }
	}//class InvestorType
}//ns
