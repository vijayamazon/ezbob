namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class LoadCustomerMarketplaceSecurityData : AStrategy {
		#region public

		#region constructor

		public LoadCustomerMarketplaceSecurityData(int nCustomerID, AConnection oDB, ASafeLog oLog) : this(nCustomerID, "", null, oDB, oLog) {
		} // constructor

		public LoadCustomerMarketplaceSecurityData(
			int nCustomerID,
			string sDisplayName,
			Guid? oInternalID,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			Result = new List<ResultRow>();

			m_oSp = new SpLoadCustomerMarketplaceSecurityData(nCustomerID, DB, Log) {
				DisplayName = sDisplayName,
				InternalID =  oInternalID,
			};
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "LoadCustomerMarketplaceSecurityData"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Result = m_oSp.Fill<ResultRow>();
		} // Execute

		#endregion method Execute

		#region Result

		public List<ResultRow> Result { get; private set; } // Result

		#endregion Result

		#region class ResultRow

		public class ResultRow : AResultRow {
			public int CustomerMarketplaceID { get; set; }
			public string DisplayName { get; set; }
			public string MarketplaceType { get; set; }
			public DateTime? UpdatingStart { get; set; }
			public byte[] SecurityData { get; set; }
			public Guid InternalID { get; set; }
		} // ResultRow

		#endregion class ResultRow

		#endregion public

		#region private

		private readonly SpLoadCustomerMarketplaceSecurityData m_oSp;

		#region class SpLoadCustomerMarketplaceSecurityData

		private class SpLoadCustomerMarketplaceSecurityData : AStoredProc {
			public SpLoadCustomerMarketplaceSecurityData(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerID = nCustomerID;
				DisplayName = string.Empty;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			#region property DisplayName

			[UsedImplicitly]
			public string DisplayName {
				get { return string.IsNullOrWhiteSpace(m_sDisplayName) ? null : m_sDisplayName; }
				set { m_sDisplayName = string.IsNullOrWhiteSpace(value) ? null : value; }
			} // DisplayName

			private string m_sDisplayName;

			#endregion property DisplayName

			#region property InternalID

			[UsedImplicitly]
			public Guid? InternalID {
				get { return (m_oInternalID == Guid.Empty) ? null : m_oInternalID; }
				set { m_oInternalID = (value == Guid.Empty) ? null : value; }
			} // InternalID

			private Guid? m_oInternalID;

			#endregion property InternalID
		} // class SpLoadCustomerMarketplaceSecurityData

		#endregion class SpLoadCustomerMarketplaceSecurityData

		#endregion private
	} // class LoadCustomerMarketplaceSecurityData
} // namespace
