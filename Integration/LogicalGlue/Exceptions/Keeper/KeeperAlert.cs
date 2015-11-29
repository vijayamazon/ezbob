namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using System;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class KeeperAlert : LogicalGlueAlert {
		[StringFormatMethod("format")]
		public KeeperAlert(ASafeLog log, string format, params object[] args) : base(log, format, args) {
		} // constructor

		[StringFormatMethod("format")]
		public KeeperAlert(
			ASafeLog log,
			Exception inner,
			string format,
			params object[] args
		) : base(log, inner, format, args) {
		} // constructor

		protected override Severity Severity {
			get { return Severity.Alert; }
		} // Severity
	} // class KeeperAlert
} // namespace
