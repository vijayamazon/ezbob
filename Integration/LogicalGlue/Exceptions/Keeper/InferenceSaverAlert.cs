namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using System;
	using Ezbob.Logger;

	public class InferenceSaverAlert : LogicalGlueAlert {
		public InferenceSaverAlert(int customerID, long requestID, Exception inner, ASafeLog log) : base(
			log,
			inner,
			"Failed to save inference for customer {0} and request {1}.",
			customerID,
			requestID
		) { } // constructor
	} // class InferenceSaverAlert
} // namespace
