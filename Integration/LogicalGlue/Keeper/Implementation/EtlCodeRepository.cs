namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;

	internal class EtlCodeRepository : AEnum<EtlCode, long> {
		public EtlCodeRepository(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public EtlCode FindOrSave(ConnectionWrapper con, string etlCode) {
			SafeReader sr = DB.GetFirst(
				con,
				"LogicalGlueFindOrSaveEtlCode",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CommunicationCode", etlCode)
			);

			return sr.IsEmpty ? null : sr.Stuff<EtlCode>();
		} // FindOrSave

		protected override ALogicalGlueStoredProc LoadAllStoredProc {
			get { return new LoadEtlCodes(DB, Log); }
		} // LoadAllStoredProc
	} // class EtlCodeRepository
} // namespace
