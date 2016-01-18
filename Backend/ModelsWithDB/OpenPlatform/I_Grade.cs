namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_Grade {
        [DataMember]
		public int GradeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public decimal? UpperBound { get; set; }
	}//class I_Grade
}//ns
