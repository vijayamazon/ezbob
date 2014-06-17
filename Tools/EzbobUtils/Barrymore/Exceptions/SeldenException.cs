namespace Ezbob.Utils.Exceptions {
	using System;

	public class SeldenException : Exception {
		public SeldenException(string sMsg) : base(sMsg) {
		} // constructor

		public SeldenException(string sMsg, Exception oInner) : base(sMsg, oInner) {
		} // constructor
	} // class SeldenException
} // namespace Ezbob.Utils
