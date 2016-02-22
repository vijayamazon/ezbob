namespace Ezbob.Integration.LogicalGlue.Exceptions.Harvester {
	using System.Collections.Generic;
	using Ezbob.Logger;

	public class BadInputDataAlert : HarvesterAlert {
		public BadInputDataAlert(List<string> errors, ASafeLog log)
			: base(log, "Bad input data specified: {0}", string.Join(" ", errors))
		{} // constructor
	} // class NoInputDataAlert
} // namespace
