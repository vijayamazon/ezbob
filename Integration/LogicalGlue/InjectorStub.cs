namespace Ezbob.Integration.LogicalGlue {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using Ezbob.Logger;

	static class InjectorStub {
		public static IKeeper GetKeeper() {
			return new Ezbob.Integration.LogicalGlue.Keeper.Implementation.Keeper(GetDBConnection(), GetLog());
		} // GetKeeper

		public static IHarvester GetHarvester() {
			return new Harvester.Implementation.Harvester(GetLog());
		} // GetHarvester

		private static ASafeLog GetLog() {
			return Library.Instance.Log;
		} // GetLog

		private static AConnection GetDBConnection() {
			return Library.Instance.DB;
		} // GetDBConnection
	} // class InjectorStub
} // namespace
