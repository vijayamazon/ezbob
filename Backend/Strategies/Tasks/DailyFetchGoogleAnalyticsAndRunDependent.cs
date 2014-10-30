namespace EzBob.Backend.Strategies.Tasks {
	using EzBob.Backend.Strategies.Misc;
	using EzBob.Backend.Strategies.Reports;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DailyFetchGoogleAnalyticsAndRunDependent : AStrategy {
		#region constructor

		public DailyFetchGoogleAnalyticsAndRunDependent(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "DailyFetchGoogleAnalyticsAndRunDependent"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			new UpdateGoogleAnalytics(null, null, DB, Log).Execute();
			new Alibaba(null, false, DB, Log).Execute();
		} // Execute

		#endregion method Execute
	} // class DailyFetchGoogleAnalyticsAndRunDependent
} // namespace
