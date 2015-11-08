namespace Ezbob.Integration.LogicalGlue {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.HarvesterInterface;
	using Ezbob.Integration.LogicalGlue.Harvester;
	using Ezbob.Integration.LogicalGlue.Keeper;
	using Ezbob.Integration.LogicalGlue.KeeperInterface;
	using log4net;

	static class InjectorStub {
		public static IKeeper GetKeeper() {
			return new DBKeeper();
		} // GetKeeper

		public static IHarvester GetHarvester() {
			return new RestHarvester();
		} // GetHarvester

		public static ILog GetLog() {
			return LogManager.GetLogger(typeof(InjectorStub));
		} // GetLog

		public static AConnection GetDBConnection() {
			return Library.Instance.DB;
		} // GetDBConnection
	} // class InjectorStub
} // namespace
