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
		public int? FundingTypeID { get; set; }

		[FK("CustomerOrigin", "CustomerOriginID")]
		[DataMember]
		public int OriginID { get; set; }

		[FK("LoanSource", "LoanSourceID")]
		[DataMember]
		public int LoanSourceID { get; set; }

		[DataMember]
		public bool IsRegulated { get; set; }
		
		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class I_ProductSubType
}//ns
