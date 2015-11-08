namespace Ezbob.Integration.LogicalGlue.Keeper {
	using System;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Integration.LogicalGlue.KeeperInterface;

	class DBKeeper : IKeeper {
		public DBKeeper() {
		} // constructor

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// </summary>
		public Inference LoadInference(int customerID, DateTime time) {
			return null; // TODO
		} // LoadInference
	} // class DBKeeper
} // namespace
