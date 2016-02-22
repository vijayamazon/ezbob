namespace Ezbob.Integration.LogicalGlue.Exceptions.Harvester {
	using System;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class HarvesterAlert : LogicalGlueAlert {
		[StringFormatMethod("format")]
		public HarvesterAlert(ASafeLog log, string format, params object[] args) : base(log, format, args) {
		} // constructor

		[StringFormatMethod("format")]
		public HarvesterAlert(
			ASafeLog log,
			Exception inner,
			string format,
			params object[] args
		) : base(log, inner, format, args) {
		} // constructor

		protected override Severity Severity {
			get { return Severity.Alert; }
		} // Severity
	} // class HarvesterAlert
} // namespace
