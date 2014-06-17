namespace Ezbob.Utils.Exceptions {
	using System;

	public abstract class AException : Exception {
		protected AException(string sMsg) : base(sMsg) {
		} // constructor

		protected AException(string sMsg, Exception oInnerException) : base(sMsg, oInnerException) {
		} // constructor
	} // class AException
} // namespace
