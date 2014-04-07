namespace Ezbob.Utils {
	using System;

	public class NullSeldenException : SeldenException {
		public NullSeldenException(string sMsg) : base(sMsg) {
		} // constructor

		public NullSeldenException(string sMsg, Exception oInner) : base(sMsg, oInner) {
		} // constructor
	} // class NullSeldenException
} // namespace Ezbob.Utils
