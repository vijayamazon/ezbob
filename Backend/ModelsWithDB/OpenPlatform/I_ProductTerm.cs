namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_ProductTerm {
        [PK(true)]
        [DataMember]
		public int ProductTermID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public int FromMonths { get; set; }

		[DataMember]
		public int ToMonths { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class I_ProductTerm
}//ns
