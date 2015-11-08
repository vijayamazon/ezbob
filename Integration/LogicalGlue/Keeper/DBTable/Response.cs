namespace Ezbob.Integration.LogicalGlue.Keeper.DBTable {
	using System;

	class Response : AWithResponseID {
		public long ServiceLogID { get; set; }
		public DateTime ReceivingTime { get; set; }
		public long RequestTypeID { get; set; }
		public long? InferenceResultEncoded { get; set; }
		public string InferenceResultDecoded { get; set; }
		public decimal? Score { get; set; }
		public string Status { get; set; }
		public string Exception { get; set; }
		public string ErrorCode { get; set; }
		public Guid? Uuid { get; set; }
	} // class Response
} // namespace
