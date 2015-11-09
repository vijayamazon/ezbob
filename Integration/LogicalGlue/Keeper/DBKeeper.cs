namespace Ezbob.Integration.LogicalGlue.Keeper {
	using System;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Integration.LogicalGlue.KeeperInterface;
	using Ezbob.Logger;
	using log4net;

	class DBKeeper : IKeeper {
		public DBKeeper(AConnection db, ILog log) {
			this.db = db;
			this.log = new SafeILog(log);
		} // constructor

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// </summary>
		public Inference LoadInference(int customerID, DateTime time) {
			return new InferenceLoader(this.db, this.log, customerID, time).Execute().Result;
		} // LoadInference

		private readonly AConnection db;
		private readonly ASafeLog log;
	} // class DBKeeper
} // namespace
