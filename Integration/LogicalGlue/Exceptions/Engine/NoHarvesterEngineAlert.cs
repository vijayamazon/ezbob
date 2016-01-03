namespace Ezbob.Integration.LogicalGlue.Exceptions.Engine {
	using Ezbob.Logger;

	public class NoHarvesterEngineAlert : LogicalGlueAlert {
		public NoHarvesterEngineAlert(ASafeLog log) : base(log, "No data harvester specified.") { } // constructor
	} // class NoHarvesterEngineAlert
} // namespace
