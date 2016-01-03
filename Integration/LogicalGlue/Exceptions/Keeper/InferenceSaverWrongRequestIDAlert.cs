namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using Ezbob.Logger;

	public class InferenceSaverNoDataAlert : LogicalGlueAlert {
		public InferenceSaverNoDataAlert(long requestID, ASafeLog log) : base(
			log,
			null,
			"Failed to save inference for request {0}: no response data specified.",
			requestID
		) { } // constructor
	} // class InferenceSaverNoDataAlert
} // namespace
