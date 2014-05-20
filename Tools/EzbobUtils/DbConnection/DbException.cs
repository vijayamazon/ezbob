namespace Ezbob.Database {
	using System;

	public class DbException : Exception {
		public DbException(string sMsg) : base(sMsg) {} // constructor

		public DbException(string sMsg, Exception oInner) : base(sMsg, oInner) {} // constructor
	} // class DbException
} // namespace Ezbob.Database
