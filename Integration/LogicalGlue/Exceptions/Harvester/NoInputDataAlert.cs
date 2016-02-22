namespace Ezbob.Integration.LogicalGlue.Exceptions.Harvester {
	using Ezbob.Logger;

	public class NoInputDataAlert : HarvesterAlert {
		public NoInputDataAlert(ASafeLog log) : base(log, "No input data specified.") {
		} // constructor
	} // class NoInputDataAlert
} // namespace
