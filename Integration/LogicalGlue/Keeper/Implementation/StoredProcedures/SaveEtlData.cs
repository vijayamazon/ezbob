namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	using DbEtlData = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.EtlData;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveEtlData : ALogicalGlueStoredProc {
		public SaveEtlData(
			long responseID,
			Response<Reply> response,
			EtlCodeRepository etlCodeRepo,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			if ((response == null) || !response.Parsed.HasEtl())
				return;

			this.etlCode = response.Parsed.Etl.Code;

			this.dbe = new DbEtlData {
				ResponseID = responseID,
				Message = response.Parsed.Etl.Message,
			};

			Tbl = new List<DbEtlData> { this.dbe, };

			this.etlCodeRepo = etlCodeRepo;
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbEtlData> Tbl { get; set; }

		public long Execute(ConnectionWrapper con) {
			FindOrSaveEtlCode(con);
			return ExecuteNonQuery(con);
		} // Execute

		private void FindOrSaveEtlCode(ConnectionWrapper con) {
			if (string.IsNullOrEmpty(this.etlCode))
				return;

			this.dbe.EtlCodeID = this.etlCodeRepo.FindOrSave(con, this.etlCode);
		} // FindOrSaveEtlCode

		private readonly EtlCodeRepository etlCodeRepo;
		private readonly DbEtlData dbe;
		private readonly string etlCode;
	} // class SaveEtlData
} // namespace
