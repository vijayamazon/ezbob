namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using System;

	class ModelOutput {
		public long ResponseID { get; set; }
		public long ModelID { get; set; }
		public long? InferenceResultEncoded { get; set; }
		public string InferenceResultDecoded { get; set; }
		public decimal? Score { get; set; }
		public string Status { get; set; }
		public string Exception { get; set; }
		public string ErrorCode { get; set; }
		public Guid? Uuid { get; set; }
	} // class ModelOutput
} // namespace
