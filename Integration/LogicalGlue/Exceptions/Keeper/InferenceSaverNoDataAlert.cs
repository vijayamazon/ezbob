namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using Ezbob.Logger;

	public class InferenceSaverWrongRequestIDAlert : LogicalGlueAlert {
		public InferenceSaverWrongRequestIDAlert(long requestID, ASafeLog log) : base(
			log,
			null,
			"Failed to save inference due to wrong request ID: {0}.",
			requestID
		) { } // constructor
	} // class InferenceSaverWrongRequestIDAlert
} // namespace
