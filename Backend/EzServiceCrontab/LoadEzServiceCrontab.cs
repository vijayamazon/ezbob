namespace EzServiceCrontab {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadEzServiceCrontab : AStoredProcedure {

		public LoadEzServiceCrontab(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters

		public bool IncludeRunning { get; set; }
	} // class LoadEzServiceCrontab
} // namespace
