namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using System;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	public class InferenceRequestSaverAlert : LogicalGlueAlert {
		public InferenceRequestSaverAlert(int customerID, InferenceInput request, Exception inner, ASafeLog log) : base(
			log,
			inner,
			"Failed to save the following inference request for customer {0}: '{1}'.",
			customerID,
			request.ToShortString()
		) { } // constructor
	} // class InferenceRequestSaverAlert
} // namespace
