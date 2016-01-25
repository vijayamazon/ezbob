namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	using DbWarning = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.Warning;
	using HarvesterWarning = Ezbob.Integration.LogicalGlue.Harvester.Interface.Warning;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveWarning : ALogicalGlueStoredProc {
		public SaveWarning(
			SortedDictionary<ModelNames, long> map,
			Response<Reply> response,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			if ((map == null) || (map.Count < 1) || (response == null) || !response.Parsed.HasInference())
				return;

			Tbl = new List<DbWarning>();

			ModelNames[] allModelNames = (ModelNames[])Enum.GetValues(typeof(ModelNames));

			foreach (ModelNames name in allModelNames) {
				var model = response.Parsed.GetParsedModel(name);

				if (model != null)
					Tbl.AddRange(Create(map, name, model.Warnings));
			} // for each model name
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbWarning> Tbl { get; set; }

		private static IEnumerable<DbWarning> Create(
			SortedDictionary<ModelNames, long> map,
			ModelNames name,
			IEnumerable<HarvesterWarning> lst
		) {
			if (lst == null)
				yield break;

			foreach (var v in lst) {
				yield return new DbWarning {
					ModelOutputID = map[name],
					FeatureName = v.FeatureName,
					MaxValue = v.MaxValue,
					MinValue = v.MinValue,
					Value = v.Value,
				};
			} // for each
		} // Create
	} // class SaveWarning
} // namespace
