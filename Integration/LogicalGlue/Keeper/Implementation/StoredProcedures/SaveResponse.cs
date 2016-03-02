namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	using DbResponse = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.Response;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveResponse : ALogicalGlueStoredProc {
		public SaveResponse(
			long requestID,
			Response<Reply> response,
			BucketRepository bucketRepo,
			TimeoutSourceRepository timeoutSourceRepo,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			Tbl = null;

			if (response == null)
				return;

			this.timeoutSourceRepo = timeoutSourceRepo;

			this.dbr = new DbResponse {
				ServiceLogID = requestID,
				HttpStatus = (int)response.Status,
				ReceivedTime = DateTime.UtcNow,
			};

			if (response.ParsingException == null) {
				this.dbr.ParsingExceptionType = null;
				this.dbr.ParsingExceptionMessage = null;
			} else {
				this.dbr.ParsingExceptionType = response.ParsingException.GetType().FullName;
				this.dbr.ParsingExceptionMessage = response.ParsingException.Message;
			} // if

			if (response.Parsed.Exists()) {
				Bucket bucket = null;

				if ((bucketRepo != null) && response.Parsed.HasDecision())
					bucket = bucketRepo.Find(response.Parsed.Inference.Decision.Bucket);

				this.timeoutSource = response.Parsed.Timeout;

				this.dbr.ResponseStatus = (int)response.Parsed.Status;
				this.dbr.ErrorMessage = response.Parsed.Error;
				this.dbr.BucketID = bucket == null ? (int?)null : bucket.Value;
				this.dbr.HasEquifaxData = response.Parsed.HasEquifaxData();
				this.dbr.Reason = response.Parsed.Reason;
				this.dbr.Outcome = response.Parsed.Outcome;
			} else {
				this.timeoutSource = null;

				this.dbr.ResponseStatus = 0;
				this.dbr.TimeoutSourceID = null;
				this.dbr.ErrorMessage = null;
				this.dbr.BucketID = null;
				this.dbr.HasEquifaxData = false;
				this.dbr.Reason = null;
				this.dbr.Outcome = null;
			} // if

			Tbl = new List<DbResponse> { this.dbr, };
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbResponse> Tbl { get; set; }

		public long Execute(ConnectionWrapper con) {
			FindOrSaveTimeoutSource(con);
			return ExecuteScalar<long>(con);
		} // Execute

		private void FindOrSaveTimeoutSource(ConnectionWrapper con) {
			if (string.IsNullOrWhiteSpace(this.timeoutSource))
				return;

			this.dbr.TimeoutSourceID = this.timeoutSourceRepo.FindOrSave(con, this.timeoutSource); 
		} // FindOrSaveTimeoutSource

		private readonly string timeoutSource;
		private readonly TimeoutSourceRepository timeoutSourceRepo;
		private readonly DbResponse dbr;
	} // class SaveResponse
} // namespace
