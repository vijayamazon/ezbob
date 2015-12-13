namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_ProductSubType {
        [PK(true)]
        [DataMember]
		public int ProductSubTypeID { get; set; }

		[FK("I_ProductType", "ProductTypeID")]
		[DataMember]
		public int ProductTypeID { get; set; }

		[FK("I_ProductTerm", "ProductTermID")]
		[DataMember]
		public int FundingTypeID { get; set; }

		[Length(255)]
		[DataMember]
		public string Name { get; set; }
		
		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class I_ProductSubType
}//ns
