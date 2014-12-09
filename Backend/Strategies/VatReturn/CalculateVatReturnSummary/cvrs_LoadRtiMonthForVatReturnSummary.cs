namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class LoadRtiMonthForVatReturnSummary : AStoredProcedure {

			public LoadRtiMonthForVatReturnSummary(int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerMarketplaceID = nCustomerMarketplaceID;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerMarketplaceID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; }

			public class ResultRow : AResultRow {
				[UsedImplicitly]
				public int CustomerID { get; set; }

				[UsedImplicitly]
				public int RecordID { get; set; }

				[UsedImplicitly]
				public DateTime DateStart { get; set; }

				[UsedImplicitly]
				public decimal AmountPaid { get; set; }

				[UsedImplicitly]
				public string CurrencyCode { get; set; }
			} // class ResultRow

		} // class LoadRtiMonthForVatReturnSummary
	} // class CalculateVatReturnSummary
} // namespace
