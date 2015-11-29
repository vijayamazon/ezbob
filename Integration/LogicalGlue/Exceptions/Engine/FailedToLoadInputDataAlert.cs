namespace Ezbob.Integration.LogicalGlue.Exceptions.Engine {
	using Ezbob.Logger;

	public class FailedToLoadInputDataAlert : LogicalGlueAlert {
		public FailedToLoadInputDataAlert(ASafeLog log, int customerID) : base(
			log,
			"Failed to load input data for customer {0}.",
			customerID
		) { } // constructor
	} // class FailedToLoadInputDataAlert
} // namespace
