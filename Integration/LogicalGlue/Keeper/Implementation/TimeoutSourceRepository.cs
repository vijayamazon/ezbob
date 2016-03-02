namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;

	internal class TimeoutSourceRepository : AEnum<TimeoutSource, long> {
		public TimeoutSourceRepository(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public TimeoutSource FindOrSave(ConnectionWrapper con, string timeoutSource) {
			SafeReader sr = DB.GetFirst(
				con,
				"LogicalGlueFindOrSaveTimeoutSource",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CommunicationCode", timeoutSource)
			);

			return sr.IsEmpty ? null : sr.Stuff<TimeoutSource>();
		} // FindOrSave

		protected override ALogicalGlueStoredProc LoadAllStoredProc {
			get { return new LoadTimeoutSources(DB, Log); }
		} // LoadAllStoredProc
	} // class TimeoutSourceRepository
} // namespace
