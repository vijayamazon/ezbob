namespace Ezbob.Utils {
	using System;

	#region class SeldenException

	public class SeldenException : Exception {
		#region public

		#region constructor

		public SeldenException(string sMsg) : base(sMsg) {
		} // constructor

		public SeldenException(string sMsg, Exception oInner) : base(sMsg, oInner) {
		} // constructor

		#endregion constructor

		#endregion public
	} // class SeldenException

	#endregion class SeldenException
} // namespace Ezbob.Utils
