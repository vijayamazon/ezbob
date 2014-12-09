using System;

namespace Ezbob.ExperianParser {

	public class OwnException : Exception {
		public OwnException(string sMsg, params object[] args) : base(string.Format(sMsg, args)) {}

		public OwnException(Exception oInner, string sMsg, params object[] args) : base(string.Format(sMsg, args), oInner) {}
	} // class OwnException

} // namespace Ezbob.ExperianParser
