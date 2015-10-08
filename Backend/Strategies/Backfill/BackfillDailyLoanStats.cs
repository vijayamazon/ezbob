namespace Ezbob.Backend.Strategies.Backfill {
	using System;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Strategies.Tasks;

	public class BackfillDailyLoanStats : AStrategy {
		public override string Name {
			get { return "Backfill daily loan stats"; }
		} // Name

		public override void Execute() {
			DateTime today = DateTime.UtcNow.Date;
			DateTime theBeginning = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);

			for (DateTime idx = theBeginning; idx <= today; idx = idx.AddDays(1)) {
				try {
					new UpdateDailyLoanStats(0, idx).Execute();
				} catch (Exception e) {
					Log.Warn(e, "Failed to update daily loan stats for date '{0}'.", idx.DateStr());
				} // try
			} // for
		} // Execute
	} // class BackfillDailyLoanStats
} // namespace

