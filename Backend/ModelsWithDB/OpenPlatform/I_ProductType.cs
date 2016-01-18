namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_ProductType {
        [PK(true)]
        [DataMember]
		public int ProductTypeID { get; set; }

		[FK("I_Product", "ProductID")]
		[DataMember]
		public int ProductID { get; set; }

		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class I_ProductType
}//ns
