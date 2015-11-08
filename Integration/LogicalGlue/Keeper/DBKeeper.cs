namespace Ezbob.Integration.LogicalGlue.Keeper {
	using System;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Integration.LogicalGlue.KeeperInterface;
	using Ezbob.Logger;
	using log4net;

	class DBKeeper : IKeeper {
		public DBKeeper() {
			Inject();
		} // constructor

		[Injected]
		public AConnection DB { get; set; }

		[Injected]
		public ILog LogWriter { get; set; }

		public ASafeLog Log {
			get {
				if (this.log == null)
					this.log = new SafeILog(LogWriter);

				return this.log;
			} // get
		} // Log

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// </summary>
		public Inference LoadInference(int customerID, DateTime time) {
			return new InferenceLoader(this, customerID, time).Execute().Result;
		} // LoadInference

		/// <summary>
		/// This is a temporary method that emulates injection.
		/// Once real injection is in place this method should be removed.
		/// </summary>
		private void Inject() {
			LogWriter = InjectorStub.GetLog();
			DB = InjectorStub.GetDBConnection();
		} // Inject

		private ASafeLog log;
	} // class DBKeeper
} // namespace
