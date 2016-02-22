namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using JetBrains.Annotations;
	using StructureMap;

	public class MarketplaceInstantUpdate : AStrategy {
		public MarketplaceInstantUpdate(int nMarketplaceID) {
			m_oStart = new StartMarketplaceUpdate(DB, Log) { MarketplaceID = nMarketplaceID };
			m_oEnd = new EndMarketplaceUpdate(DB, Log) { MarketplaceID = nMarketplaceID };
		} // constructor

		public override string Name {
			get { return "Marketplace Instant Update"; }
		} // Name

		public override void Execute() {
			Start();
			End(null, 0);
		} // Execute

		public void Start() {
			SafeReader sr = m_oStart.GetFirst();

			m_oEnd.HistoryRecordID = sr["HistoryRecordID"];
			m_oEnd.MarketplaceTypeID = sr["MarketplaceTypeID"];
		} // Start

		public void End(string sError, int nTokenExpired) {
			// flush the session after updating mp
			try {
				var customerMarketPlaceRepository = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>();
				var mp = customerMarketPlaceRepository.Get(this.m_oStart.MarketplaceID);
				customerMarketPlaceRepository.SaveOrUpdate(mp);
			} catch (Exception e) {
				Log.Alert(e, "Something went really-really not cool while updating marketplace, ignoring it for now.");
			} // try

			Log.Info("Executing end update for mp {0} ", m_oStart.MarketplaceID);
			m_oEnd.ErrorMessage = sError;
			m_oEnd.TokenExpired = nTokenExpired;
			m_oEnd.ExecuteNonQuery();
		} // End

		private readonly StartMarketplaceUpdate m_oStart;
		private readonly EndMarketplaceUpdate m_oEnd;

		// ReSharper disable ValueParameterNotUsed

		private class StartMarketplaceUpdate : AStoredProcedure {
			public StartMarketplaceUpdate(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return MarketplaceID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int MarketplaceID { get; set; }

			[UsedImplicitly]
			public DateTime UpdatingStart {
				get { return DateTime.UtcNow; }
				set { }
			} // UpdatingStart
		} // class StartMarketplaceUpdate

		private class EndMarketplaceUpdate : AStoredProcedure {
			public EndMarketplaceUpdate(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return (MarketplaceID > 0) && (HistoryRecordID > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public int MarketplaceID { get; set; }

			[UsedImplicitly]
			public int HistoryRecordID { get; set; }

			[UsedImplicitly]
			public Guid MarketplaceTypeID { get; set; }

			[UsedImplicitly]
			public DateTime UpdatingEnd {
				get { return DateTime.UtcNow; }
				set { }
			} // UpdatingEnd

			[UsedImplicitly]
			public string ErrorMessage { get; set; }

			[UsedImplicitly]
			public int TokenExpired { get; set; }
		} // class EndMarketplaceUpdate

		// ReSharper restore ValueParameterNotUsed
	} // class MarketplaceInstantUpdate
} // namespace Ezbob.Backend.Strategies
