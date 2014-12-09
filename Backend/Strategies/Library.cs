namespace Ezbob.Backend.Strategies {
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.LibraryGlobals;

	public class Library : GloLib {
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

		public static Library Instance {
			get {
				Library lib;

				lock (lockInstance)
					lib = Library.instance;

				return lib;
			} // get
		} // Instance

		private Library() {
		} // constructor

		private static Library instance;
		private static readonly object lockInstance;
		private static bool isInitialized;
	} // class Library
} // namespace
