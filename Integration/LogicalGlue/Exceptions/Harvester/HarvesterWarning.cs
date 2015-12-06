namespace Ezbob.Integration.LogicalGlue.Exceptions.Harvester {
	using System;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class HarvesterWarning : LogicalGlueWarning {
		[StringFormatMethod("format")]
		public HarvesterWarning(ASafeLog log, string format, params object[] args) : base(log, format, args) {
		} // constructor

		[StringFormatMethod("format")]
		public HarvesterWarning(
			ASafeLog log,
			Exception inner,
			string format,
			params object[] args
		) : base(log, inner, format, args) {
		} // constructor

		protected override Severity Severity {
			get { return Severity.Warn; }
		} // Severity
	} // class HarvesterWarning
} // namespace
