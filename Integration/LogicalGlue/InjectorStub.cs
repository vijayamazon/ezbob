namespace Ezbob.Integration.LogicalGlue {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using Ezbob.Logger;

	public static class InjectorStub {
		public static IEngine GetTestEngine() {
			return new Engine.Implementation.Engine(GetKeeper(), GetTestHarvester(), GetLog());
		} // GetTestEngine

		public static IEngine GetEngine() {
			return new Engine.Implementation.Engine(GetKeeper(), GetHarvester(), GetLog());
		} // GetEngine

		public static IKeeper GetKeeper() {
			return new Ezbob.Integration.LogicalGlue.Keeper.Implementation.Keeper(GetDBConnection(), GetLog());
		} // GetKeeper

		public static IHarvester GetHarvester() {
			return new Harvester.Implementation.Harvester(GetLog());
		} // GetHarvester

		public static IHarvester GetTestHarvester() {
			return new Harvester.Implementation.TestHarvester(GetLog());
		} // GetTestHarvester

		private static ASafeLog GetLog() {
			return Library.Instance.Log;
		} // GetLog

		private static AConnection GetDBConnection() {
			return Library.Instance.DB;
		} // GetDBConnection
	} // class InjectorStub
} // namespace
