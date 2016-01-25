namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	using DbEncodingFailure = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.EncodingFailure;
	using HarvesterEncodingFailure = Ezbob.Integration.LogicalGlue.Harvester.Interface.EncodingFailure;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveEncodingFailure : ALogicalGlueStoredProc {
		public SaveEncodingFailure(
			SortedDictionary<ModelNames, long> map,
			Response<Reply> response,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			if ((map == null) || (map.Count < 1) || (response == null) || !response.Parsed.HasInference())
				return;

			Tbl = new List<DbEncodingFailure>();

			ModelNames[] allModelNames = (ModelNames[])Enum.GetValues(typeof(ModelNames));

			foreach (ModelNames name in allModelNames) {
				var model = response.Parsed.GetParsedModel(name);

				if (model != null)
					Tbl.AddRange(Create(map, name, model.EncodingFailures));
			} // for each model name
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbEncodingFailure> Tbl { get; set; }

		private static IEnumerable<DbEncodingFailure> Create(
			SortedDictionary<ModelNames, long> map,
			ModelNames name,
			IEnumerable<HarvesterEncodingFailure> lst
		) {
			if (lst == null)
				yield break;

			foreach (var v in lst) {
				yield return new DbEncodingFailure {
					ColumnName = v.ColumnName,
					Message = v.Message,
					ModelOutputID = map[name],
					Reason = v.Reason,
					RowIndex = v.RowIndex,
					UnencodedValue = v.UnencodedValue,
				};
			} // for each
		} // Create
	} // class SaveEncodingFailure
} // namespace
