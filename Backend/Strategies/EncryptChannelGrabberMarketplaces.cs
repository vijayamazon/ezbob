namespace EzBob.Backend.Strategies {
	using System;
	using System.Data;
	using System.Text;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using Integration.ChannelGrabberConfig;
	using JetBrains.Annotations;

	public class EncryptChannelGrabberMarketplaces : AStrategy {
		#region public

		#region constructor

		public EncryptChannelGrabberMarketplaces(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Encrypt Channel Grabber Marketplaces"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oBatchID = Guid.NewGuid();

			m_nEncryptionCounter = 0;

			Integration.ChannelGrabberConfig.Configuration.GetInstance(Log).ForEachVendor(ProcessVendor);

			Log.Debug("{0} accounts have been encrypted.", m_nEncryptionCounter);

			if (m_nEncryptionCounter > 0) {
				var sp = new ApplyEncryptedMarketplaceSecurityData(DB, Log) {
					BatchID = m_oBatchID,
				};

				sp.ExecuteNonQuery();
			} // if
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private int m_nEncryptionCounter;
		private Guid m_oBatchID;

		#region method ProcessVendor

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

		#endregion method ProcessVendor

		#region class ApplyEncryptedMarketplaceSecurityData

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

		#endregion class ApplyEncryptedMarketplaceSecurityData

		#region class EncryptMarketplaceSecurityData

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

		#endregion class EncryptMarketplaceSecurityData

		#region class LoadMarketplaceSecurityData

		private class LoadMarketplaceSecurityData : AStoredProcedure {
			public LoadMarketplaceSecurityData(Guid nMarketplaceType, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				MarketplaceType = nMarketplaceType;
			} // constructor

			public override bool HasValidParameters() {
				return MarketplaceType != Guid.Empty;
			} // HasValidParameters

			[UsedImplicitly] 
			public Guid MarketplaceType { get; set; }

			#region class ResultRow

			public class ResultRow : AResultRow {
				public int ID { get; [UsedImplicitly] set; }

				public byte[] SecurityData { get; [UsedImplicitly] set; }
			} // class ResultRow

			#endregion class ResultRow
		} // class LoadMarketplaceSecurityData

		#endregion class LoadMarketplaceSecurityData

		#endregion private
	} // class EncryptChannelGrabberMarketplaces
} // namespace EzBob.Backend.Strategies
