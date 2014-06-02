namespace EzService.Exceptions {
	using System;
	using EzBob.Backend.Strategies.Exceptions;

	/// <summary>
	/// Quiet exception. Writes nothing to log on creation.
	/// </summary>
	public class ServiceException : AStrategyException {
		public ServiceException(string sMsg) : base(null, sMsg) {
		} // constructor

		public ServiceException(string sMsg, Exception oInnerException) : base(null, sMsg, oInnerException) {
		} // constructor
	} // class ServiceException
} // namespace
