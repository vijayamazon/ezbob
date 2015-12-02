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
						HttpStatus = response.Parsed.Exists() ? (int)response.Parsed.Status : 0,
						BucketID = response.Parsed.HasInference() ? (int)response.Parsed.Inference.Bucket : (long?)null,
						HasEquifaxData = response.Parsed.HasEquifaxData(),
						ReceivedTime = DateTime.UtcNow,
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
