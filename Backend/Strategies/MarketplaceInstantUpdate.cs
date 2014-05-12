namespace EzBob.Backend.Strategies {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class MarketplaceInstantUpdate : AStrategy {
		#region public

		#region constructor

		public MarketplaceInstantUpdate(int nMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oStart = new StartMarketplaceUpdate(DB, Log) { MarketplaceID = nMarketplaceID };
			m_oEnd = new EndMarketplaceUpdate(DB, Log) { MarketplaceID = nMarketplaceID };
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Marketplace Instant Update"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Start();
			End(null, 0);
		} // Execute

		#endregion method Execute

		#region method Start

		public void Start() {
			m_oEnd.HistoryRecordID = (int)m_oStart.ExecuteScalar<decimal>();
		} // Start

		#endregion method Start

		#region method End

		public void End(string sError, int nTokenExpired) {
			m_oEnd.ErrorMessage = sError;
			m_oEnd.TokenExpired = nTokenExpired;
			m_oEnd.ExecuteNonQuery();
		} // End

		#endregion method End

		#endregion public

		#region private

		private readonly StartMarketplaceUpdate m_oStart;
		private readonly EndMarketplaceUpdate m_oEnd;

// ReSharper disable ValueParameterNotUsed
		#region class StartMarketplaceUpdate

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

		#endregion class StartMarketplaceUpdate

		#region class EndMarketplaceUpdate

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
			public DateTime UpdatingEnd {
				get { return DateTime.UtcNow; }
				set { }
			} // UpdatingEnd

			[UsedImplicitly]
			public string ErrorMessage { get; set; }

			[UsedImplicitly]
			public int TokenExpired { get; set; }
		} // class EndMarketplaceUpdate

		#endregion class EndMarketplaceUpdate
// ReSharper restore ValueParameterNotUsed

		#endregion private
	} // class MarketplaceInstantUpdate
} // namespace EzBob.Backend.Strategies
