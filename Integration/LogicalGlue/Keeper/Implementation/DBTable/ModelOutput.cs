namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using System;
	using Ezbob.Database;
	using Ezbob.Utils.dbutils;

	class ModelOutput {
		[FieldName("ModelOutputID")]
		[PK(true)]
		public long ID { get; set; }

		public long ResponseID { get; set; }
		public long ModelID { get; set; }
		public long? InferenceResultEncoded { get; set; }
		[Length(255)]
		public string InferenceResultDecoded { get; set; }
		public decimal? Score { get; set; }
		[Length(255)]
		public string Status { get; set; }
		[Length(LengthType.MAX)]
		public string Exception { get; set; }
		[Length(LengthType.MAX)]
		public string ErrorCode { get; set; }
		public Guid? Uuid { get; set; }
	} // class ModelOutput
} // namespace
