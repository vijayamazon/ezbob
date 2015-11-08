namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_Index {
        [PK(true)]
        [DataMember]
		public int IndexID { get; set; }

		[FK("I_Investor", "InvestorID")]
        [DataMember]
        public int InvestorID { get; set; }

		[FK("I_ProductType", "ProductTypeID")]
		[DataMember]
		public int ProductTypeID { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[DataMember]
		public decimal GradeA { get; set; }
		
		[DataMember]
		public decimal GradeB { get; set; }
		
		[DataMember]
		public decimal GradeC { get; set; }
		
		[DataMember]
		public decimal GradeD { get; set; }
	
		[DataMember]
		public decimal GradeE { get; set; }

		[DataMember]
		public decimal GradeF { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class I_Index
}//ns
