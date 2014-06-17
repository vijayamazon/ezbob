namespace Ezbob.Utils.Exceptions {
	using System;

	public class QuietException : AException {
		public QuietException(string sMsg, Exception oInnerException = null) : base(sMsg, oInnerException) {} // constructor
	} // class QuietException
} // namespace
