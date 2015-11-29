namespace Ezbob.Integration.LogicalGlue.Exceptions.Engine {
	using Ezbob.Logger;

	public class NoConnectionEngineAlert : LogicalGlueAlert {
		public NoConnectionEngineAlert(ASafeLog log) : base(log, "No database connection specified.") { } // constructor
	} // class NoConnectionEngineAlert
} // namespace
