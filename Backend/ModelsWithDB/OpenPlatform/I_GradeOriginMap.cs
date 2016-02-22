namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_GradeOriginMap {
        [PK(true)]
        [DataMember]
		public int GradeOriginMapID { get; set; }

		[FK("I_Grade", "GradeID")]
		[DataMember]
		public int GradeID { get; set; }

		[FK("CustomerOrigin", "CustomerOriginID")]
		[DataMember]
		public int OriginID { get; set; }
	}//class I_GradeOriginMap
}//ns
