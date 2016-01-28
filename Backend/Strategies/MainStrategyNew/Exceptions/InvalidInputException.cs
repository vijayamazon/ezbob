namespace Ezbob.Backend.Strategies.MainStrategyNew.Exceptions {
	using System;

	internal class InvalidInputException : Exception {
		public InvalidInputException(string format, params object[] args) : base(string.Format(format, args)) {
		} // constructor
	} // class InvalidInputException
} // namespace
