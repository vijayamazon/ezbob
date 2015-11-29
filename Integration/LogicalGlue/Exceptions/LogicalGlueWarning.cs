namespace Ezbob.Integration.LogicalGlue.Exceptions {
	using System;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class LogicalGlueWarning : LogicalGlueException {
		[StringFormatMethod("format")]
		public LogicalGlueWarning(ASafeLog log, string format, params object[] args) : base(log, format, args) {
		} // constructor

		[StringFormatMethod("format")]
		public LogicalGlueWarning(
			ASafeLog log,
			Exception inner,
			string format,
			params object[] args
		) : base(log, inner, format, args) {
		} // constructor

		protected override Severity Severity {
			get { return Severity.Warn; }
		} // Severity
	} // class LogicalGlueWarning
} // namespace
