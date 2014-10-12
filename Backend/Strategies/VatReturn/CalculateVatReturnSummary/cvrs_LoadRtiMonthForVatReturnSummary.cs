namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class LoadRtiMonthForVatReturnSummary : AStoredProcedure {
			#region constructor

			public LoadRtiMonthForVatReturnSummary(int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerMarketplaceID = nCustomerMarketplaceID;
			} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return CustomerMarketplaceID > 0;
			} // HasValidParameters

			#endregion method HasValidParameters

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; }

			#region class ResultRow

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

			#endregion class ResultRow
		} // class LoadRtiMonthForVatReturnSummary
	} // class CalculateVatReturnSummary
} // namespace
