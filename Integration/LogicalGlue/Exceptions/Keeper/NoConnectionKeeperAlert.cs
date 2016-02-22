namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using Ezbob.Logger;

	public class NoConnectionKeeperAlert : LogicalGlueAlert {
		public NoConnectionKeeperAlert(ASafeLog log) : base(log, "No database connection specified.") { } // constructor
	} // class NoConnectionKeeperAlert
} // namespace
