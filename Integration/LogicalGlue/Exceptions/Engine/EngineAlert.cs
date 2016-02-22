namespace Ezbob.Integration.LogicalGlue.Exceptions.Engine {
	using System;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class EngineAlert : LogicalGlueAlert {
		[StringFormatMethod("format")]
		public EngineAlert(ASafeLog log, string format, params object[] args) : base(log, format, args) {
		} // constructor

		[StringFormatMethod("format")]
		public EngineAlert(
			ASafeLog log,
			Exception inner,
			string format,
			params object[] args
		) : base(log, inner, format, args) {
		} // constructor

		protected override Severity Severity {
			get { return Severity.Alert; }
		} // Severity
	} // class EngineAlert
} // namespace
