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
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			if ((response == null) || !response.Parsed.HasEtl())
				return;

			Tbl = new List<DbEtlData> {
				new DbEtlData {
					ResponseID = responseID,
					EtlCodeID = (int?)response.Parsed.Etl.Code,
					Message = response.Parsed.Etl.Message,
				},
			};
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbEtlData> Tbl { get; set; }
	} // class SaveEtlData
} // namespace
