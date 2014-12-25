namespace Ezbob.Models {
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.LibraryGlobals;

	public class Library : GloLib {
		public static Library Instance {
			get {
				Library lib;

				lock (lockInstance)
					lib = Library.instance;

				return lib;
			} // get
		}

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
		} // Initialize

		// Instance

		private Library() {
		} // constructor

		private static readonly object lockInstance;
		private static Library instance;
		private static bool isInitialized;
	} // class Library
} // namespace
