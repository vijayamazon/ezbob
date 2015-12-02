namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveRawResponse : ALogicalGlueStoredProc {
		public SaveRawResponse(
			long requestID,
			Response<Reply> response,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			ServiceLogID = requestID;
			RawResponse = response == null ? null : response.RawReply;
		} // constructor

		public override bool HasValidParameters() {
			return (ServiceLogID > 0);
		} // HasValidParameters

		public long ServiceLogID { get; set; }
		public string RawResponse { get; set; }
	} // class SaveRawResponse
} // namespace
