namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadInputData : ACustomerTimeStoredProc {
		public LoadInputData(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public bool MonthlyRepaymentOnly { get; set; }
	} // class LoadInputData
} // namespace
