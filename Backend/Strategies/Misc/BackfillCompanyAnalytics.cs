namespace EzBob.Backend.Strategies.Misc {
	using System;
	using Experian;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillCompanyAnalytics : AStrategy {
		public BackfillCompanyAnalytics(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oParser = new ExperianParserForAnalytics(DB, Log);
		} // constructor

		public override string Name {
			get { return "BackfillCompanyAnalytics"; }
		} // Name

		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int customerId = sr["CustomerId"];

					try {
						m_oParser.UpdateAnalytics(customerId);
					}
					catch (Exception ex) {
						Log.Error(ex, "The backfill for customer {0} failed.", customerId);
					} // try

					return ActionResult.Continue;
				},
				"GetAllCustomersWithCompany",
				CommandSpecies.StoredProcedure
			);
		} // Execute

		private readonly ExperianParserForAnalytics m_oParser;
	} // class BackfillCompanyAnalytics
} // namespace
