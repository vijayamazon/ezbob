namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadInferenceIfExists : LoadInference {
		public LoadInferenceIfExists(AConnection db, ASafeLog log) : base(db, log) { }
	} // class LoadInferenceIfExists
} // namespace
