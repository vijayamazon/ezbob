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
		public OriginationTime(ASafeLog log) {
			this.row = null;
			this.log = log.Safe();
		} // constructor

		/// <summary>
		/// Time of the first transaction across all customer marketplaces.
		/// </summary>
		public DateTime? Since {
			get { return this.row == null ? null : this.row.Time; }
		} // Since

		/// <summary>
		/// Customer business seniority as number of days between today and the first transaction time.
		/// </summary>
		public int Seniority {
			get { return Since == null ? 0 : (int)(DateTime.UtcNow - Since.Value).TotalDays; }
		} // Seniority

		/// <summary>
		/// Feeds marketplace first transaction time.
		/// </summary>
		/// <param name="sr">Transaction time read from DB. Refer to <see cref="Row"/> for details.</param>
		public void Process(SafeReader sr) {
			Row r = sr.Fill<Row>().SetTime();

			this.log.Debug("Origination time: processing row {0}", r);

			if (r.Time == null)
				return;

			if (Since == null) {
				this.log.Debug("Marketplace origination time set from {0}.", r);
				this.row = r;
				return;
			} // if

			if (r.Time < Since.Value) {
				this.row = r;
				this.log.Debug("Marketplace origination time updated from {0}.", r);
			} // if
		} // Process

		/// <summary>
		/// Adds Experian business incorporation date to origination time calculation.
		/// </summary>
		/// <param name="oIncorporationDate">Business incorporation data received from Experian.</param>
		public void FromExperian(DateTime? oIncorporationDate) {
			if (oIncorporationDate == null)
				return;

			if ((Since == null) || (oIncorporationDate.Value < Since.Value)) {
				this.row = new Row {
					OneTime = oIncorporationDate,
					IsPaymentAccount = false,
					MarketplaceID = 0,
					MarketplaceType = "Experian",
				};

				this.row.SetTime();

				this.log.Debug("Marketplace origination time updated from {0}.", this.row);
			} // if
		} // FromExperian

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

			/// <summary>
			/// Detects whether this marketplace should be included into business seniority calculation.
			/// </summary>
			private bool IsIncluded {
				get {
					if (InternalID == companyFiles)
						return false;

					return !IsPaymentAccount || (InternalID == payPal) || (InternalID == hmrc);
				} // get
			} // IsIncluded

			public override string ToString() {
				return string.Format("time: {0}, mp id: {1}, mp type: {2}", Time, MarketplaceID, MarketplaceType);
			} // ToString

			private static readonly Guid payPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");
			private static readonly Guid hmrc = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");
			private static readonly Guid companyFiles = new Guid("1C077670-6D6C-4CE9-BEBC-C1F9A9723908");
		} // class Row

		private string MarketplaceType {
			get { return this.row == null ? "not set" : this.row.MarketplaceType; }
		} // MarketplaceType

		private int MarketplaceID {
			get { return this.row == null ? 0 : this.row.MarketplaceID; }
		} // MarketplaceID

		private Row row;
		private readonly ASafeLog log;
	} // class OriginationTime
} // namespace
