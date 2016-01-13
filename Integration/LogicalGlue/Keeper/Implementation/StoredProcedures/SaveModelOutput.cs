namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	using DbModelOutput = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.ModelOutput;
	using HarvesterModelOutput = Ezbob.Integration.LogicalGlue.Harvester.Interface.ModelOutput;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveModelOutput : ALogicalGlueStoredProc {
		public SaveModelOutput(
			long responseID,
			Response<Reply> response,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			if ((response == null) || !response.Parsed.HasInference())
				return;

			Tbl = new List<DbModelOutput> {
				Create(responseID, ModelNames.FuzzyLogic, response.Parsed.FuzzyLogic),
				Create(responseID, ModelNames.NeuralNetwork, response.Parsed.NeuralNetwork),
			}
				.Where(m => m != null)
				.ToList();
		} // constructor

		public override bool HasValidParameters() {
			return (Tbl != null) && (Tbl.Count > 0);
		} // HasValidParameters

		public List<DbModelOutput> Tbl { get; set; }

		private static DbModelOutput Create(long responseID, ModelNames name, HarvesterModelOutput mo) {
			return (mo == null) ? null : new DbModelOutput {
				ErrorCode = mo.ErrorCode,
				Exception = mo.Exception,
				InferenceResultDecoded = mo.DecodedResult,
				InferenceResultEncoded = mo.EncodedResult,
				ModelID = (int)name,
				ResponseID = responseID,
				Score = mo.Score,
				Status = mo.Status,
				Uuid = mo.Uuid,
			};
		} // Create
	} // class SaveModelOutput
} // namespace
