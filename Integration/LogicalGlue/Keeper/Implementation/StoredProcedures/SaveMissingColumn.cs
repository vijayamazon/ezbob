namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	using DbMissingColumn = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.MissingColumn;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveMissingColumn : ALogicalGlueStoredProc {
		public SaveMissingColumn(
			SortedDictionary<ModelNames, long> map,
			Response<Reply> response,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			if ((map == null) || (map.Count < 1) || (response == null) || !response.Parsed.HasInference())
				return;

			Tbl = new List<DbMissingColumn>();

			ModelNames[] allModelNames = (ModelNames[])Enum.GetValues(typeof(ModelNames));

			foreach (ModelNames name in allModelNames) {
				var model = response.Parsed.GetParsedModel(name);

				if (model != null)
					Tbl.AddRange(Create(map, name, model.MissingColumns));
			} // for each model name
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbMissingColumn> Tbl { get; set; }

		private static IEnumerable<DbMissingColumn> Create(
			SortedDictionary<ModelNames, long> map,
			ModelNames name,
			IEnumerable<string> lst
		) {
			if (lst == null)
				yield break;

			foreach (var v in lst) {
				yield return new DbMissingColumn {
					ColumnName = v,
					ModelOutputID = map[name],
				};
			} // for each
		} // Create
	} // class SaveEncodingFailure
} // namespace
