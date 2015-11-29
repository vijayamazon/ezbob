namespace Ezbob.Integration.LogicalGlue.Exceptions {
	using System;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class LogicalGlueAlert : LogicalGlueException {
		[StringFormatMethod("format")]
		public LogicalGlueAlert(ASafeLog log, string format, params object[] args) : base(log, format, args) {
		} // constructor

		[StringFormatMethod("format")]
		public LogicalGlueAlert(
			ASafeLog log,
			Exception inner,
			string format,
			params object[] args
		) : base(log, inner, format, args) {
		} // constructor

		protected override Severity Severity {
			get { return Severity.Alert; }
		} // Severity
	} // class LogicalGlueAlert
} // namespace
