namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using System;
	using System.Globalization;
	using Ezbob.Logger;

	public class InferenceLoaderAlert : LogicalGlueAlert {
		public InferenceLoaderAlert(int customerID, DateTime time, Exception inner, ASafeLog log) : base(
			log,
			inner,
			"Failed to load inference for customer {0} at {1}.",
			customerID,
			time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
		) { } // constructor
	} // class InferenceLoaderAlert
} // namespace
