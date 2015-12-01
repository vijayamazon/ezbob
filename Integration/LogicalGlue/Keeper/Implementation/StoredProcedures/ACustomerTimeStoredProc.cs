namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class ACustomerTimeStoredProc : ACustomerStoredProc {
		protected ACustomerTimeStoredProc(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public DateTime Now { get; set; }
	} // class ACustomerTimeStoredProc
} // namespace
