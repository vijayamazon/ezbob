namespace Ezbob.Integration.LogicalGlue.Exceptions.Harvester {
	using Ezbob.Logger;

	public class NoConfigurationAlert : HarvesterAlert {
		public NoConfigurationAlert(ASafeLog log) : base(log, "No remote server configuration specified.") {
		} // constructor
	} // class NoConfigurationAlert
} // namespace
