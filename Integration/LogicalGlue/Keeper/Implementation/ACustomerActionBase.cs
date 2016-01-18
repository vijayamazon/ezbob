namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class ACustomerActionBase : AActionBase {
		protected ACustomerActionBase(AConnection db, ASafeLog log, int customerID) : base(db, log) {
			CustomerID = customerID;
		} // constructor

		protected int CustomerID { get; private set; }
	} // class ACustomerActionBase
} // namespace
