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
		public int ResponseStatus { get; set; }
		public long? TimeoutSourceID { get; set; }

		[Length(LengthType.MAX)]
		public string ErrorMessage { get; set; }

		[FieldName("GradeID")]
		public int? BucketID { get; set; }

		public bool HasEquifaxData { get; set; }

		[Length(LengthType.MAX)]
		public string ParsingExceptionType { get; set; }

		[Length(LengthType.MAX)]
		public string ParsingExceptionMessage { get; set; }

		[Length(LengthType.MAX)]
		public string Reason { get; set; }

		[Length(LengthType.MAX)]
		public string Outcome { get; set; }
	} // class Response
} // namespace
