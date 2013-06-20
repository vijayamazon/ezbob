using System;

namespace Ezbob.Database {
	#region class DbException

	public class DbException : Exception {
		#region public

		#region constructor

		public DbException(string sMsg) : base(sMsg) {} // constructor

		public DbException(string sMsg, Exception oInner) : base(sMsg, oInner) {} // constructor

		#endregion constructor

		#endregion public
	} // class DbException

	#endregion class DbException
} // namespace Ezbob.Database
