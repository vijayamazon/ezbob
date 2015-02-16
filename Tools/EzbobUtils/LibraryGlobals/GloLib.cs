namespace Ezbob.LibraryGlobals {
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;

	public abstract class GloLib {
		public virtual ASafeLog Log { get; protected set; }
		public virtual AConnection DB { get; protected set; }
		public virtual Ezbob.Context.Environment Env { get; protected set; }
		public virtual CultureInfo Culture { get; protected set; }

		public virtual void Init(Ezbob.Context.Name name, string variant, ASafeLog log = null) {
			Log = log ?? new SafeLog();
			Env = new Ezbob.Context.Environment(name, variant, Log);
			DB = new SqlConnection(Env, Log);
			Culture = CreateCultureInfo();
		} // Init

		public virtual void Init(Ezbob.Context.Environment env, AConnection db, ASafeLog log) {
			Log = log;
			Env = env;
			DB = db;
			Culture = CreateCultureInfo();
		} // Init

		public virtual void Init(ASafeLog log = null) {
			Log = log ?? new SafeLog();
			Env = new Ezbob.Context.Environment(Log);
			DB = new SqlConnection(Env, Log);
			Culture = CreateCultureInfo();
		} // Init

		protected virtual CultureInfo CreateCultureInfo() {
			return new CultureInfo("en-GB", false);
		} // CreateCultureInfo
	} // class GloLib
} // namespace
