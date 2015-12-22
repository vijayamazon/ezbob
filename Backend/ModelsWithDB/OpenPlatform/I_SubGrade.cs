namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_SubGrade {
        [PK(true)]
        [DataMember]
		public int SubGradeID { get; set; }

		[FK("I_Grade", "GradeID")]
		[DataMember]
		public int GradeID { get; set; }

		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public decimal? MinScore { get; set; }

		[DataMember]
		public decimal? MaxScore { get; set; }
	}//class I_SubGrade
}//ns
