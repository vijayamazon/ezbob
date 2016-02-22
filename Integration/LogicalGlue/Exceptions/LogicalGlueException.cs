namespace Ezbob.Integration.LogicalGlue.Exceptions {
	using System;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public abstract class LogicalGlueException : Exception {
		[StringFormatMethod("format")]
		protected LogicalGlueException(
			ASafeLog log,
			string format,
			params object[] args
		) : base(string.Format(format, args)) {
			Log(log);
		} // constructor

		[StringFormatMethod("format")]
		protected LogicalGlueException(
			ASafeLog log,
			Exception inner,
			string format,
			params object[] args
		) : base(string.Format(format, args), inner) {
			Log(log);
		} // constructor

		protected abstract Severity Severity { get; }

		private void Log(ASafeLog log) {
			if (log == null)
				return;

			log.Say(Severity, this, "{0}", Message);
		} // Log
	} // class LogicalGlueException
} // namespace
