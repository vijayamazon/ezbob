namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData {
	using System;
	using System.Collections.Generic;

	public class MarketplaceData {
		public MarketplaceData(RawSource src) {
			MonthTurnover = new SortedDictionary<int, decimal>();

			if ((src.MpID == null) || (src.MpTypeInternalID == null)) // should never happen because of checks in the caller
				throw new Exception("Marketplace not specified.");

			ID = src.MpID.Value;
			Type = new MpType(src.MpTypeInternalID.Value, src.MpType);
			TotalsMonth = src.MpTotalsMonth;
		} // constructor

		public MpType Type { get; private set; }
		public int ID { get; private set; }
		public DateTime TotalsMonth { get; private set; }
		public SortedDictionary<int, decimal> MonthTurnover { get; private set; }

		public decimal? GetTurnover(int distance) {
			return MonthTurnover.ContainsKey(distance) ? MonthTurnover[distance] : (decimal?)null;
		} // GetTurnover

		public class MpType {
			public MpType(Guid id, string name) {
				ID = id;
				Name = name;
			} // constructor

			public string Name { get; private set; }
			public Guid ID { get; private set; }
		} // class MpType
	} // class MarketplaceData
} // namespace
