namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadInference : ACustomerTimeStoredProc {
		public LoadInference(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor
	} // class LoadInference
} // namespace
