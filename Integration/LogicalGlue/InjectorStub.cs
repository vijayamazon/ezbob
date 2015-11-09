namespace Ezbob.Integration.LogicalGlue {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Implementation;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using log4net;

	static class InjectorStub {
		public static IKeeper GetKeeper() {
			return new Ezbob.Integration.LogicalGlue.Keeper.Implementation.Keeper(GetDBConnection(), GetLog());
		} // GetKeeper

		public static IHarvester GetHarvester() {
			return new RestHarvester(GetLog());
		} // GetHarvester

		private static ILog GetLog() {
			return LogManager.GetLogger(typeof(InjectorStub));
		} // GetLog

		private static AConnection GetDBConnection() {
			return Library.Instance.DB;
		} // GetDBConnection
	} // class InjectorStub
} // namespace
