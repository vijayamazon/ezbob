namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
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

			Tbl.AddRange(Create(map, ModelNames.FuzzyLogic, response.Parsed.Inference.FuzzyLogic.EncodingFailures));
			Tbl.AddRange(Create(map, ModelNames.NeuralNetwork, response.Parsed.Inference.NeuralNetwork.EncodingFailures));
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
