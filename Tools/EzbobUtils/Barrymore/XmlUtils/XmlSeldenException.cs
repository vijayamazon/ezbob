namespace Ezbob.Utils.XmlUtils {
	using System;
	using Exceptions;

	public class XmlSeldenException : SeldenException {
		public XmlSeldenException(string sMsg) : base(sMsg) {
		} // constructor

		public XmlSeldenException(string sMsg, Exception oInner) : base(sMsg, oInner) {
		} // constructor
	} // class XmlSeldenException
} // namespace Ezbob.Utils.XmlUtils
