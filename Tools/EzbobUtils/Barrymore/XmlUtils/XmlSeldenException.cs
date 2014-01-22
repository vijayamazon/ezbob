namespace Ezbob.Utils.XmlUtils {
	using System;

	#region class XmlSeldenException

	public class XmlSeldenException : SeldenException {
		#region public

		#region constructor

		public XmlSeldenException(string sMsg) : base(sMsg) {
		} // constructor

		public XmlSeldenException(string sMsg, Exception oInner) : base(sMsg, oInner) {
		} // constructor

		#endregion constructor

		#endregion public
	} // class XmlSeldenException

	#endregion class XmlSeldenException
} // namespace Ezbob.Utils.XmlUtils
