namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	using DbOutputRatio = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.OutputRatio;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveOutputRatio : ALogicalGlueStoredProc {
		public SaveOutputRatio(
			SortedDictionary<ModelNames, long> map,
			Response<Reply> response,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			if ((map == null) || (map.Count < 1) || (response == null) || !response.Parsed.HasInference())
				return;

			Tbl = new List<DbOutputRatio>();

			Tbl.AddRange(Create(map, ModelNames.FuzzyLogic, response.Parsed.Inference.FuzzyLogic.OutputRatios));
			Tbl.AddRange(Create(map, ModelNames.NeuralNetwork, response.Parsed.Inference.NeuralNetwork.OutputRatios));
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbOutputRatio> Tbl { get; set; }

		private static IEnumerable<DbOutputRatio> Create(
			SortedDictionary<ModelNames, long> map,
			ModelNames name,
			Dictionary<string, decimal> lst
		) {
			if (lst == null)
				yield break;

			foreach (var v in lst) {
				yield return new DbOutputRatio {
					ModelOutputID = map[name],
					OutputClass = v.Key,
					Score = v.Value,
				};
			} // for each
		} // Create
	} // class SaveEncodingFailure
} // namespace
