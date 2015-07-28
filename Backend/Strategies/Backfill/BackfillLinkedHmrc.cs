namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class BackfillLinkedHmrc : AStrategy {
		public BackfillLinkedHmrc() {
		} // constructor

		public override string Name {
			get { return "BackfillLinkedHmrc"; }
		} // Name

		public override void Execute() {
			this.pc = new ProgressCounter("{0} marketplaces processed.", Log, 10);

			DB.ForEachRowSafe(UpdateMp, Query, CommandSpecies.Text);

			this.pc.Log();
		} // Execute

		private void UpdateMp(SafeReader sr) {
			int customerID = sr["CustomerId"];
			int mpID = sr["MpID"];

			try {
				new UpdateMarketplace(customerID, mpID, false).Execute();
			} catch (Exception e) {
				Log.Warn(e, "Failed to update marketplace {0} of customer {1}.", mpID, customerID);
			} // try

			this.pc++;
		} // UpdateMp

		private ProgressCounter pc;

		private const string Query = @"
SELECT
	m.CustomerId,
	MpID = m.Id
FROM
	MP_CustomerMarketPlace m
	INNER JOIN MP_MarketplaceType t
		ON m.MarketPlaceId = t.Id
		AND t.InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'
	INNER JOIN Customer c
		ON m.CustomerId = c.Id
		AND c.IsTest = 0
WHERE
	m.DisplayName NOT LIKE '%@%'
	AND
	NOT EXISTS (SELECT * FROM MP_VatReturnRecords r WHERE r.CustomerMarketPlaceId = m.Id)
";
	} // class BackfillLinkedHmrc
} // namespace

