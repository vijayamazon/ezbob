namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadInference : ACustomerTimeStoredProc {
		public LoadInference(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return (ResponseID > 0) || base.HasValidParameters();
		} // HasValidParameters

		public long ResponseID { get; set; }
	} // class LoadInference
} // namespace
