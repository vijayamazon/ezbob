namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class ACustomerTimeStoredProc : ALogicalGlueStoredProc {
		public override bool HasValidParameters() {
			return CustomerID > 0;
		} // HasValidParameters

		protected ACustomerTimeStoredProc(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public int CustomerID { get; set; }
		public DateTime Now { get; set; }
	} // class ACustomerTimeStoredProc
} // namespace
