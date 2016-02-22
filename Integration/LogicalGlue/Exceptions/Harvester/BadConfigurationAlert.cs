namespace Ezbob.Integration.LogicalGlue.Exceptions.Harvester {
	using System.Collections.Generic;
	using Ezbob.Logger;

	public class BadConfigurationAlert : HarvesterAlert {
		public BadConfigurationAlert(List<string> errors, ASafeLog log)
			: base(log, "Bad remote server configuration specified: {0}", string.Join(" ", errors))
		{} // constructor
	} // class NoConfigurationAlert
} // namespace
