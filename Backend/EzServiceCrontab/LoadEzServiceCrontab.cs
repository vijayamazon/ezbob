namespace EzServiceCrontab {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadEzServiceCrontab : AStoredProcedure {
		#region constructor

		public LoadEzServiceCrontab(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region method HasValidParameters

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters

		#endregion method HasValidParameters

		public bool IncludeRunning { get; set; }
	} // class LoadEzServiceCrontab
} // namespace
