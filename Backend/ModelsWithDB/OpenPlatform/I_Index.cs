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
		public decimal GradeAPercent { get; set; }

		[DataMember]
		public decimal GradeAMinScore { get; set; }

		[DataMember]
		public decimal GradeAMaxScore { get; set; }

		[DataMember]
		public decimal GradeBPercent { get; set; }

		[DataMember]
		public decimal GradeBMinScore { get; set; }

		[DataMember]
		public decimal GradeBMaxScore { get; set; }

		[DataMember]
		public decimal GradeCPercent { get; set; }

		[DataMember]
		public decimal GradeCMinScore { get; set; }

		[DataMember]
		public decimal GradeCMaxScore { get; set; }

		[DataMember]
		public decimal GradeDPercent { get; set; }

		[DataMember]
		public decimal GradeDMinScore { get; set; }

		[DataMember]
		public decimal GradeDMaxScore { get; set; }

		[DataMember]
		public decimal GradeEPercent { get; set; }

		[DataMember]
		public decimal GradeEMinScore { get; set; }

		[DataMember]
		public decimal GradeEMaxScore { get; set; }

		[DataMember]
		public decimal GradeFPercent { get; set; }

		[DataMember]
		public decimal GradeFMinScore { get; set; }

		[DataMember]
		public decimal GradeFMaxScore { get; set; }

		[DataMember]
		public decimal GradeGPercent { get; set; }

		[DataMember]
		public decimal GradeGMinScore { get; set; }

		[DataMember]
		public decimal GradeGMaxScore { get; set; }

		[DataMember]
		public decimal GradeHPercent { get; set; }

		[DataMember]
		public decimal GradeHMinScore { get; set; }

		[DataMember]
		public decimal GradeHMaxScore { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class I_Index
}//ns
