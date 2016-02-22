namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Text;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using global::Integration.ChannelGrabberConfig;
	using JetBrains.Annotations;

	public class EncryptChannelGrabberMarketplaces : AStrategy {
		public override string Name {
			get { return "Encrypt Channel Grabber Marketplaces"; }
		} // Name

		public override void Execute() {
			m_oBatchID = Guid.NewGuid();

			m_nEncryptionCounter = 0;

			global::Integration.ChannelGrabberConfig.Configuration.GetInstance(Log).ForEachVendor(ProcessVendor);

			Log.Debug("{0} accounts have been encrypted.", m_nEncryptionCounter);

			if (m_nEncryptionCounter > 0) {
				var sp = new ApplyEncryptedMarketplaceSecurityData(DB, Log) {
					BatchID = m_oBatchID,
				};

				sp.ExecuteNonQuery();
			} // if
		} // Execute

		private int m_nEncryptionCounter;
		private Guid m_oBatchID;

		private void ProcessVendor(VendorInfo vi) {
			Log.Debug("Processing {0} marketplaces started...", vi.Name);

			var sp = new LoadMarketplaceSecurityData(new Guid(vi.InternalID), DB, Log);

			sp.ForEachResult<LoadMarketplaceSecurityData.ResultRow>(mp => {
				var sSecurityData = Encoding.ASCII.GetString(mp.SecurityData);
				Log.Debug("{0}: {1}", mp.ID, sSecurityData);

				if (sSecurityData.IndexOf("<AccountModel") < 0)
					Log.Debug("Already encrypted, skipping.");
				else {
					string sEncrypted = new Encrypted(sSecurityData);

					Log.Debug("Encrypted: {0}", sEncrypted);

					var upd = new EncryptMarketplaceSecurityData(DB, Log) {
						BatchID = m_oBatchID,
						MarketplaceID = mp.ID,
						OldData = Encoding.ASCII.GetBytes(sSecurityData),
						NewData = Encoding.ASCII.GetBytes(sEncrypted),
					};

					try {
						upd.ExecuteNonQuery();
						m_nEncryptionCounter++;
					}
					catch (Exception e) {
						Log.Error(e, "Failed to save encrypted data for marketplace {0}.", mp.ID);
					} // try
				} // if

				return ActionResult.Continue;
			});

			Log.Debug("Processing {0} marketplaces complete.", vi.Name);
		} // ProcessVendor

		private class ApplyEncryptedMarketplaceSecurityData : AStoredProcedure {
			public ApplyEncryptedMarketplaceSecurityData(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				DoRollback = false;
			} // constructor

			public override bool HasValidParameters() {
				return BatchID != Guid.Empty;
			} // HasValidParameters

			[UsedImplicitly] 
			public Guid BatchID { get; set; }

			[UsedImplicitly] 
			public bool DoRollback { get; set; }
		} // ApplyEncryptedMarketplaceSecurityData

		private class EncryptMarketplaceSecurityData : AStoredProcedure {
			public EncryptMarketplaceSecurityData(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return
					(BatchID != Guid.Empty) &&
					(MarketplaceID > 0) && 
					(OldData != null) &&
					(NewData != null);
			} // HasValidParameters

			[UsedImplicitly] 
			public Guid BatchID { get; set; }

			[UsedImplicitly] 
			public int MarketplaceID { get; set; }

			[UsedImplicitly]
			public DateTime BackupTime {
				get { return DateTime.UtcNow; }
				set { }
			} // BackupTime

			[UsedImplicitly] 
			public byte[] OldData { get; set; }

			[UsedImplicitly] 
			public byte[] NewData { get; set; }
		} // EncryptMarketplaceSecurityData

		private class LoadMarketplaceSecurityData : AStoredProcedure {
			public LoadMarketplaceSecurityData(Guid nMarketplaceType, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				MarketplaceType = nMarketplaceType;
			} // constructor

			public override bool HasValidParameters() {
				return MarketplaceType != Guid.Empty;
			} // HasValidParameters

			[UsedImplicitly] 
			public Guid MarketplaceType { get; set; }

			public class ResultRow : AResultRow {
				public int ID { get; [UsedImplicitly] set; }

				public byte[] SecurityData { get; [UsedImplicitly] set; }
			} // class ResultRow

		} // class LoadMarketplaceSecurityData

	} // class EncryptChannelGrabberMarketplaces
} // namespace Ezbob.Backend.Strategies
