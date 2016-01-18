namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using EKM;
	using Ezbob.Database;
	using Ezbob.Utils;
	using EzBob.AmazonServiceLib;
	using EzBob.CommonLib;
	using EzBob.eBayServiceLib;
	using EzBob.PayPalServiceLib;
	using FreeAgent;
	using Sage;
	using YodleeLib.connector;

	using ChaGraConfig = global::Integration.ChannelGrabberConfig.Configuration;

	public class BackfillTurnover : AStrategy {
		public override string Name {
			get { return "BackfillTurnover"; }
		} // Name

		public BackfillTurnover() {
			this.types = new SortedDictionary<Guid, ServiceInfo>();

			Add(new AmazonServiceInfo(), "Amazon");

			ChaGraConfig.Instance.ForEachVendor(
				vi => Add(new global::Integration.ChannelGrabberFrontend.ServiceInfo(vi.Name), "ChaGra")
			);

			Add(new eBayServiceInfo(), "Ebay");
			Add(new EkmServiceInfo(), "Ekm");
			Add(new FreeAgentServiceInfo(), "FreeAgent");
			Add(new global::Integration.ChannelGrabberFrontend.ServiceInfo("HMRC"), "Hmrc");
			Add(new PayPalServiceInfo(), "PayPal");
			Add(new SageServiceInfo(), "Sage");
			Add(new YodleeServiceInfo(), "Yodlee");
		} // constructor

		public override void Execute() {
			Prerequisite();

			List<SourceData> lst = DB.Fill<SourceData>(LoadSourceQuery, CommandSpecies.Text);

			var pc = new ProgressCounter("{0} of " + lst.Count + " history items processed.", Log, 100);

			foreach (SourceData sd in lst) {
				if (!this.types.ContainsKey(sd.InternalId)) {
					Log.Alert("Unexpected marketplace type internal id: {0}", sd.InternalId);
					pc++;
					continue;
				} // if

				ServiceInfo si = this.types[sd.InternalId];
				Log.Info("Start to update for {0} with history id {1}...", si.Info.DisplayName, sd.Id);

				try {
					DB.ExecuteNonQuery(si.SpName, CommandSpecies.StoredProcedure, new QueryParameter("HistoryID", sd.Id));
					Log.Info("End to update for {0} with history id {1} completed.", si.Info.DisplayName, sd.Id);
				} catch (Exception ex) {
					Log.Warn(ex, "Update for {0} with history id {1} failed.", si.Info.DisplayName, sd.Id);
				} // try

				pc++;
			} // for each

			pc.Log();
		} // Execute

		private class ServiceInfo {
			public IMarketplaceServiceInfo Info { get; private set; }
			public string SpName { get; private set; }

			public ServiceInfo(IMarketplaceServiceInfo info, string spName) {
				Info = info;
				SpName = "UpdateMpTotals" + spName;
			} // constructor
		} // ServiceInfo

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class SourceData {
			public int Id { get; set; }
			public Guid InternalId { get; set; }
		} // class SourceData

		private void Add(IMarketplaceServiceInfo info, string spName) {
			this.types[info.InternalId] = new ServiceInfo(info, spName);
		} // Add

		private void Prerequisite() {
			DB.ExecuteNonQuery(PrerequisiteQuery, CommandSpecies.Text);
		} // Prerequisite

		private const string LoadSourceQuery = @"SELECT
	h.Id,
	t.InternalId
FROM
	MP_CustomerMarketPlaceUpdatingHistory h
	INNER JOIN MP_CustomerMarketPlace m ON h.CustomerMarketPlaceId = m.Id
	INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	t.InternalId NOT IN (
		'1c077670-6d6c-4ce9-bebc-c1f9a9723908', -- Company files
		'fc8f2710-aeda-481d-86ff-539dd1fb76e0'  -- PayPoint
	)
ORDER BY
	h.Id";

		private const string PrerequisiteQuery = @"TRUNCATE TABLE AmazonAggregation
TRUNCATE TABLE ChannelGrabberAggregation
TRUNCATE TABLE EbayAggregation
TRUNCATE TABLE EbayAggregationCategories
TRUNCATE TABLE EkmAggregation
TRUNCATE TABLE FreeAgentAggregation
TRUNCATE TABLE HmrcAggregation
TRUNCATE TABLE PayPalAggregation
TRUNCATE TABLE SageAggregation
TRUNCATE TABLE YodleeAggregation
TRUNCATE TABLE YodleeGroupAggregation
";

		private readonly SortedDictionary<Guid, ServiceInfo> types;
	} // class BackfillTurnover
} // namespace
