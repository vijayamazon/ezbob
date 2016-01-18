namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class ACustomerTimeStoredProc : ACustomerStoredProc {
		protected ACustomerTimeStoredProc(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return base.HasValidParameters() && (Now >= theBeginning);
		} // HasValidParameters

		public DateTime Now { get; set; }

		private static readonly DateTime theBeginning = new DateTime(1976, 7, 1, 9, 30, 0, DateTimeKind.Utc);
	} // class ACustomerTimeStoredProc
} // namespace
