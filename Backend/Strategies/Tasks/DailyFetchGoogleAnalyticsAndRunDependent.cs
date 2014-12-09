namespace Ezbob.Backend.Strategies.Tasks {
	using System;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.Reports;

	public class DailyFetchGoogleAnalyticsAndRunDependent : AStrategy {
		public override string Name {
			get { return "DailyFetchGoogleAnalyticsAndRunDependent"; }
		} // Name

		public override void Execute() {
			DateTime oToday = DateTime.Today;
			new UpdateGoogleAnalytics(oToday.AddDays(-2), oToday).Execute();
			new Alibaba(null, false).Execute();
		} // Execute
	} // class DailyFetchGoogleAnalyticsAndRunDependent
} // namespace
