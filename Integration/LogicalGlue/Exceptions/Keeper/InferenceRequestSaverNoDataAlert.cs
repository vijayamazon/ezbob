namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using System;
	using Ezbob.Logger;

	public class InferenceRequestSaverNoDataAlert : LogicalGlueAlert {
		public InferenceRequestSaverNoDataAlert(int customerID, Exception inner, ASafeLog log) : base(
			log,
			inner,
			"Failed to save the following inference request for customer {0}: no request specified.",
			customerID
		) { } // constructor
	} // class InferenceRequestSaverNoDataAlert
} // namespace
