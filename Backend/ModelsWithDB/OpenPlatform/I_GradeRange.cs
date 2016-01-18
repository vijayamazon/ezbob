namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_GradeRange {
        [PK(true)]
        [DataMember]
		public int GradeRangeID { get; set; }

		[FK("I_Grade", "GradeID")]
		[DataMember]
		public int? GradeID { get; set; }

		[FK("I_SubGrade", "SubGradeID")]
		[DataMember]
		public int? SubGradeID { get; set; }

		[FK("LoanSource", "GradeID")]
		[DataMember]
		public int LoanSourceID { get; set; }

		[FK("CustomerOrigin", "CustomerOriginID")]
		[DataMember]
		public int OriginID { get; set; }

		[DataMember]
		public bool IsFirstLoan { get; set; }

		[DataMember]
		public decimal MinSetupFee { get; set; }

		[DataMember]
		public decimal MaxSetupFee { get; set; }

		[DataMember]
		public decimal MinInterestRate { get; set; }

		[DataMember]
		public decimal MaxInterestRate { get; set; }

		[DataMember]
		public decimal MinLoanAmount { get; set; }

		[DataMember]
		public decimal MaxLoanAmount { get; set; }

		[DataMember]
		public int MinTerm { get; set; }

		[DataMember]
		public int MaxTerm { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class I_GradeRange
}//ns
