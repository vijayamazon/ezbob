namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveInference : ALogicalGlueStoredProc {
		public SaveInference(
			long requestID,
			Response<Reply> response,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			this.requestID = requestID;
			this.response = response;
		} // constructor

		public override bool HasValidParameters() {
			return (this.requestID > 0) && (this.response != null);
		} // HasValidParameters

		public DateTime Now {
			get { return DateTime.UtcNow; }
			set { }
		} // DateOfBirth

		private readonly long requestID;
		private readonly Response<Reply> response;
	} // class SaveInferenceRequest
} // namespace
