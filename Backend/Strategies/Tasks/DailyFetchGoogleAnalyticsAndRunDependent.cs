namespace EzBob.Backend.Strategies.Tasks {
	using System;
	using EzBob.Backend.Strategies.Misc;
	using EzBob.Backend.Strategies.Reports;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DailyFetchGoogleAnalyticsAndRunDependent : AStrategy {

		public DailyFetchGoogleAnalyticsAndRunDependent(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		public override string Name {
			get { return "DailyFetchGoogleAnalyticsAndRunDependent"; }
		} // Name

		public override void Execute() {
			DateTime oToday = DateTime.Today;
			new UpdateGoogleAnalytics(oToday.AddDays(-2), oToday, DB, Log).Execute();
			new Alibaba(null, false, DB, Log).Execute();
		} // Execute

	} // class DailyFetchGoogleAnalyticsAndRunDependent
} // namespace
