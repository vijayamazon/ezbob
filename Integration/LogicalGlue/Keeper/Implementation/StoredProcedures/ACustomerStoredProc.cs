namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class ACustomerStoredProc : ALogicalGlueStoredProc {
		public override bool HasValidParameters() {
			return CustomerID > 0;
		} // HasValidParameters

		protected ACustomerStoredProc(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public int CustomerID { get; set; }
	} // class ACustomerStoredProc
} // namespace
