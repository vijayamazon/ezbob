namespace Ezbob.Backend.Strategies {
	using Ezbob.Database;
	using Ezbob.LibraryGlobals;
	using Ezbob.Logger;

	public class Library : GloLib {
		public static Library Instance {
			get {
				Library lib;

				lock (lockInstance)
					lib = instance;

				return lib;
			} // get
		} // Instance

		static Library() {
			instance = new Library();
			lockInstance = new object();
			isInitialized = false;
		} // static constructor

		public static void Initialize(Ezbob.Context.Environment env, AConnection db, ASafeLog log) {
			if (!isInitialized) {
				lock (lockInstance) {
					if (!isInitialized) {
						instance = new Library();
						instance.Init(env, db, log);
						isInitialized = true;
					} // if
				} // lock
			} // if

			Ezbob.Backend.Extensions.Library.Initialize(env, db, log);
			Ezbob.Models.Library.Initialize(env, db, log);
			AutomationCalculator.Library.Initialize(env, db, log);
			Ezbob.Backend.CalculateLoan.LoanCalculator.Library.Initialize(env, db, log);
			EZBob.DatabaseLib.Library.Initialize(env, db, log);
			Ezbob.Integration.LogicalGlue.Library.Initialize(env, db, log);
		} // Initialize

		private Library() {} // constructor

		private static readonly object lockInstance;
		private static Library instance;
		private static bool isInitialized;
	} // class Library
} // namespace
