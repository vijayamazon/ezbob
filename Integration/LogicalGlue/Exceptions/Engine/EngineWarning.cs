namespace Ezbob.Integration.LogicalGlue.Exceptions.Engine {
	using System;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class EngineWarning : LogicalGlueWarning {
		[StringFormatMethod("format")]
		public EngineWarning(ASafeLog log, string format, params object[] args) : base(log, format, args) {
		} // constructor

		[StringFormatMethod("format")]
		public EngineWarning(
			ASafeLog log,
			Exception inner,
			string format,
			params object[] args
		) : base(log, inner, format, args) {
		} // constructor

		protected override Severity Severity {
			get { return Severity.Warn; }
		} // Severity
	} // class EngineWarning
} // namespace
