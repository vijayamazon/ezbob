namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	internal class LoadInferenceIfExists : LoadInference {
		public LoadInferenceIfExists(AConnection db, ASafeLog log) : base(db, log) { }

		[NonTraversable]
		public override long ResponseID { get; set; }

		[NonTraversable]
		public override int HistoryLength { get; set; }
	} // class LoadInferenceIfExists
} // namespace
