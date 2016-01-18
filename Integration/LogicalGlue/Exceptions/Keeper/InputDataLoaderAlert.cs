namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using System;
	using System.Globalization;
	using Ezbob.Logger;

	public class InputDataLoaderAlert : LogicalGlueAlert {
		public InputDataLoaderAlert(int customerID, DateTime now, Exception inner, ASafeLog log) : base(
			log,
			inner,
			"Failed to load inference input data for customer {0} at {1}.",
			customerID,
			now.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
		) { } // constructor
	} // class InputDataLoaderAlert
} // namespace
