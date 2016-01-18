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
			if (response != null) {
				Tbl = new List<DbResponse> {
					new DbResponse {
						ServiceLogID = requestID,
						HttpStatus = (int)response.Status,
						ResponseStatus = response.Parsed.Exists() ? (int)response.Parsed.Status : 0,
						TimeoutSourceID = response.Parsed.Exists() ? (int?)response.Parsed.Timeout : null,
						ErrorMessage = response.Parsed.Exists() ? response.Parsed.Error : null,
						BucketID = response.Parsed.Bucket.HasValue ? (int)response.Parsed.Bucket.Value : (int?)null,
						HasEquifaxData = response.Parsed.HasEquifaxData(),
						ReceivedTime = DateTime.UtcNow,
						ParsingExceptionType = response.ParsingException == null
							? null
							: response.ParsingException.GetType().FullName,
						ParsingExceptionMessage = response.ParsingException == null
							? null
							: response.ParsingException.Message,
						Reason = response.Parsed.Reason,
						Outcome = response.Parsed.Outcome,
					}
				};
			} // if
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbResponse> Tbl { get; set; }
	} // class SaveResponse
} // namespace
