namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	using DbResponse = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.Response;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveResponse : ALogicalGlueStoredProc {
		public SaveResponse(
			long requestID,
			Response<Reply> response,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			Tbl = null;

			if (response == null)
				return;

			var dbr = new DbResponse {
				ServiceLogID = requestID,
				HttpStatus = (int)response.Status,
				ReceivedTime = DateTime.UtcNow,
			};

			if (response.ParsingException == null) {
				dbr.ParsingExceptionType = null;
				dbr.ParsingExceptionMessage = null;
			} else {
				dbr.ParsingExceptionType = response.ParsingException.GetType().FullName;
				dbr.ParsingExceptionMessage = response.ParsingException.Message;
			} // if

			if (response.Parsed.Exists()) {
				dbr.ResponseStatus = (int)response.Parsed.Status;
				dbr.TimeoutSourceID = (int?)response.Parsed.Timeout;
				dbr.ErrorMessage = response.Parsed.Error;
				dbr.BucketID = response.Parsed.Bucket.HasValue ? (int)response.Parsed.Bucket.Value : (int?)null;
				dbr.HasEquifaxData = response.Parsed.HasEquifaxData();
				dbr.Reason = response.Parsed.Reason;
				dbr.Outcome = response.Parsed.Outcome;
			} else {
				dbr.ResponseStatus = 0;
				dbr.TimeoutSourceID = null;
				dbr.ErrorMessage = null;
				dbr.BucketID = null;
				dbr.HasEquifaxData = false;
				dbr.Reason = null;
				dbr.Outcome = null;
			} // if

			Tbl = new List<DbResponse> { dbr, };
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbResponse> Tbl { get; set; }
	} // class SaveResponse
} // namespace
