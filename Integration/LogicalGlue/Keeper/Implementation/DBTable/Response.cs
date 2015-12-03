namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable {
	using System;
	using Ezbob.Database;
	using Ezbob.Utils.dbutils;

	class Response {
		[FieldName("ResponseID")]
		[PK(true)]
		public long ID { get; set; }

		public long ServiceLogID { get; set; }
		public DateTime ReceivedTime { get; set; }
		public int HttpStatus { get; set; }
		public long? TimeoutSourceID { get; set; }
		[Length(LengthType.MAX)]
		public string ErrorMessage { get; set; }
		public long? BucketID { get; set; }
		public bool HasEquifaxData { get; set; }
	} // class Response
} // namespace
