namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveRawResponse : ALogicalGlueStoredProc {
		public SaveRawResponse(
			long requestID,
			Response<Reply> response,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			ServiceLogID = requestID;

			if (response == null)
				RawResponse = null;
			else {
				RawResponse = string.IsNullOrWhiteSpace(response.RawReply)
					? null
					: new Encrypted(response.RawReply).ToString();
			} // if
		} // constructor

		public override bool HasValidParameters() {
			return (ServiceLogID > 0);
		} // HasValidParameters

		public long ServiceLogID { get; set; }
		public string RawResponse { get; set; }
	} // class SaveRawResponse
} // namespace
