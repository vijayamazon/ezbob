namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class AActionBase {
		protected AActionBase(AConnection db, ASafeLog log, int customerID) {
			Executed = false;
			DB = db;
			Log = log;
			CustomerID = customerID;
		} // constructor

		protected AConnection DB { get; private set; }
		protected ASafeLog Log { get; private set; }
		protected int CustomerID { get; private set; }
		protected bool Executed { get; set; }
	} // class InputDataLoader
} // namespace
