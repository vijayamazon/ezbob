namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class OriginationTime {
		#region public

		#region constructor

		public OriginationTime(ASafeLog oLog) {
			m_oRow = null;
			m_oLog = oLog ?? new SafeLog();
		} // constructor

		#endregion constructor

		#region property Since

		public DateTime? Since {
			get { return m_oRow == null ? null : m_oRow.Time; }
		} // Since

		#endregion property Since

		#region property Seniority

		public int Seniority {
			get { return Since == null ? 0 : (int)(DateTime.UtcNow - Since.Value).TotalDays; }
		} // Seniority

		#endregion property Seniority

		#region method Process

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

		#region class Row

		private class Row {
			public int MarketplaceID { get; [UsedImplicitly] set; }

			public string MarketplaceType { get; [UsedImplicitly] set; }

			[UsedImplicitly]
			public bool IsPaymentAccount { private get; set; }

			[UsedImplicitly]
			public Guid InternalID { private get; set; }

			[UsedImplicitly]
			public DateTime? OneTime { private get; set; }

			[UsedImplicitly]
			public DateTime? TwoTime { private get; set; }

			public DateTime? Time { get; private set; }

			#region method Time

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

			#endregion method GetTime

			#region property IsIncluded

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

		#endregion private
	} // class OriginationTime
} // namespace
