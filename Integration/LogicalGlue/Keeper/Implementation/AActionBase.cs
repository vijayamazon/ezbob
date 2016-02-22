namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class AActionBase {
		protected AActionBase(AConnection db, ASafeLog log) {
			Executed = false;
			DB = db;
			Log = log;
		} // constructor

		protected AConnection DB { get; private set; }
		protected ASafeLog Log { get; private set; }
		protected bool Executed { get; set; }
	} // class AActionBase
} // namespace
