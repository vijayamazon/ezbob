using System;

namespace Ezbob.ExperianParser {
	#region class OwnException

	public class OwnException : Exception {
		public OwnException(string sMsg, params object[] args) : base(string.Format(sMsg, args)) {}

		public OwnException(Exception oInner, string sMsg, params object[] args) : base(string.Format(sMsg, args), oInner) {}
	} // class OwnException

	#endregion class OwnException
} // namespace Ezbob.ExperianParser
