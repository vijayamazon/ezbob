namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_Grade {
        [PK(true)]
        [DataMember]
		public int GradeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }
	}//class InvestorType
}//ns
