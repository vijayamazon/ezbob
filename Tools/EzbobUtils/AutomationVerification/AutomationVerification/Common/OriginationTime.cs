namespace AutomationCalculator.Common {
	using System;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	/// <summary>
	/// Contains time of the first transaction for calculating customer business seniority.
	/// </summary>
	public class OriginationTime {
		#region public

		#region constructor

		public OriginationTime(ASafeLog oLog) {
			m_oRow = null;
			m_oLog = oLog ?? new SafeLog();
		} // constructor

		#endregion constructor

		#region property Since

		/// <summary>
		/// Time of the first transaction across all customer marketplaces.
		/// </summary>
		public DateTime? Since {
			get { return m_oRow == null ? null : m_oRow.Time; }
		} // Since

		#endregion property Since

		#region property Seniority

		/// <summary>
		/// Customer business seniority as number of days between today and the first transaction time.
		/// </summary>
		public int Seniority {
			get { return Since == null ? 0 : (int)(DateTime.UtcNow - Since.Value).TotalDays; }
		} // Seniority

		#endregion property Seniority

		#region method Process

		/// <summary>
		/// Feeds marketplace first transaction time.
		/// </summary>
		/// <param name="sr">Transaction time read from DB. Refer to <see cref="Row"/> for details.</param>
		public void Process(SafeReader sr) {
			Row r = sr.Fill<Row>().SetTime();

			if (r.Time == null)
				return;

			if (Since == null) {
				m_oRow = r;
				return;
			} // if

			if (r.Time < Since.Value) {
				m_oRow = r;
				m_oLog.Debug("Marketplace origination time updated from {0}.", r);
			} // if
		} // Process

		#endregion method Process

		#region method FromExperian

		/// <summary>
		/// Adds Experian business incorporation date to origination time calculation.
		/// </summary>
		/// <param name="oIncorporationDate">Business incorporation data received from Experian.</param>
		public void FromExperian(DateTime? oIncorporationDate) {
			if (oIncorporationDate == null)
				return;

			if ((Since == null) || (oIncorporationDate.Value < Since.Value)) {
				m_oRow = new Row {
					OneTime = oIncorporationDate,
					IsPaymentAccount = false,
					MarketplaceID = 0,
					MarketplaceType = "Experian",
				};

				m_oRow.SetTime();

				m_oLog.Debug("Marketplace origination time updated from {0}.", m_oRow);
			} // if
		} // FromExperian

		#endregion method FromExperian

		#region method ToString

		public override string ToString() {
			if (Since == null)
				return "no data available";

			return string.Format(
				"origination time {0} based on {1}{2}",
				Since.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				MarketplaceType,
				MarketplaceID == 0 ? string.Empty : "(id: " + MarketplaceID + ")"
			);
		} // ToString

		#endregion method ToString

		#region class Row

		/// <summary>
		/// Marketplace first transaction time read from DB.
		/// </summary>
		public class Row {
			/// <summary>
			/// Marketplace ID (MP_CustomerMarketplace.Id).
			/// </summary>
			public int MarketplaceID { get; [UsedImplicitly] set; }

			/// <summary>
			/// Marketplace type (MP_MarketplaceType.Name).
			/// </summary>
			public string MarketplaceType { get; [UsedImplicitly] set; }

			/// <summary>
			/// Indicates whether this marketplace is a payment account (MP_MarketplaceType.IsPaymentAccount).
			/// </summary>
			[UsedImplicitly]
			public bool IsPaymentAccount { private get; set; }

			/// <summary>
			/// Marketplace type unique ID (MP_MarketplaceType.InternalId).
			/// </summary>
			[UsedImplicitly]
			public Guid InternalID { private get; set; }

			/// <summary>
			/// One of the candidates to marketplace origination time.
			/// </summary>
			[UsedImplicitly]
			public DateTime? OneTime { private get; set; }

			/// <summary>
			/// One of the candidates to marketplace origination time.
			/// </summary>
			[UsedImplicitly]
			public DateTime? TwoTime { private get; set; }

			/// <summary>
			/// Marketplace origination time. Is calculated by <see cref="SetTime"/>.
			/// </summary>
			public DateTime? Time { get; private set; }

			#region method SetTime

			/// <summary>
			/// Calculates marketplace origination time as min(<see cref="OneTime"/>, <see cref="TwoTime"/>).
			/// </summary>
			/// <returns>Current instance (for chaining).</returns>
			public Row SetTime() {
				if (!IsIncluded) {
					Time = null;
					return this;
				} // if

				if (!OneTime.HasValue && !TwoTime.HasValue) {
					Time = null;
					return this;
				} // if

				if (OneTime.HasValue && TwoTime.HasValue) {
					Time = OneTime.Value < TwoTime.Value ? OneTime : TwoTime;
					return this;
				} // if

				Time = OneTime.HasValue ? OneTime : TwoTime;
				return this;
			} // GetTime

			#endregion method SetTime

			#region property IsIncluded

			/// <summary>
			/// Detects whether this marketplace should be included into business seniority calculation.
			/// </summary>
			private bool IsIncluded {
				get { return !IsPaymentAccount || (InternalID == PayPal) || (InternalID == Hmrc); }
			} // IsIncluded

			#endregion property IsIncluded

			#region method ToString

			public override string ToString() {
				return string.Format("time: {0}, mp id: {1}, mp type: {2}", Time, MarketplaceID, MarketplaceType);
			} // ToString

			#endregion method ToString

			private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");
			private static readonly Guid Hmrc = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");
		} // class Row

		#endregion class Row

		#endregion public

		#region private

		#region property MarketplaceType

		private string MarketplaceType {
			get { return m_oRow == null ? "not set" : m_oRow.MarketplaceType; }
		} // MarketplaceType

		#endregion property MarketplaceType

		#region property MarketplaceID

		private int MarketplaceID {
			get { return m_oRow == null ? 0 : m_oRow.MarketplaceID; }
		} // MarketplaceID

		#endregion property MarketplaceID

		private Row m_oRow;
		private readonly ASafeLog m_oLog;

		#endregion private
	} // class OriginationTime
} // namespace
