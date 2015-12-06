namespace Ezbob.Integration.LogicalGlue.Exceptions.Engine {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Logger;

	public class FailedToLoadInputDataAlert : LogicalGlueAlert {
		public FailedToLoadInputDataAlert(ASafeLog log, int customerID, DateTime now, List<string> errors) : base(
			log,
			"Failed to load input data for customer {0} at {1}: {2}",
			customerID,
			now.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
			string.Join(" ", errors)
		) { } // constructor
	} // class FailedToLoadInputDataAlert
} // namespace
