namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using System;

	class Response {
		public long ServiceLogID { get; set; }
		public DateTime ReceivedTime { get; set; }
		public decimal MonthlyRepayment { get; set; }
		public int BucketID { get; set; }
		public bool HasEquifaxData { get; set; }
	} // class Response
} // namespace
