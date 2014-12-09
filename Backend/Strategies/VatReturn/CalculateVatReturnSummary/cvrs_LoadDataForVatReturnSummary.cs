namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class LoadDataForVatReturnSummary : AStoredProcedure {

			public LoadDataForVatReturnSummary(int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerMarketplaceID = nCustomerMarketplaceID;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerMarketplaceID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; }

			public class ResultRow : AResultRow {

				[UsedImplicitly]
				public decimal Amount { get; set; }

				[UsedImplicitly]
				public string CurrencyCode { get; set; }

				[UsedImplicitly]
				public string BoxName { get; set; }

				[UsedImplicitly]
				public DateTime DateFrom { get; set; }

				[UsedImplicitly]
				public DateTime DateTo { get; set; }

				[UsedImplicitly]
				public int BusinessID { get; set; }

				public int BoxNum {
					get {
						if (!m_nBoxNum.HasValue)
							m_nBoxNum = VatReturnUtils.BoxNameToNum(BoxName);

						return m_nBoxNum.Value;
					} // get
				} // BoxNum

				private int? m_nBoxNum;

			} // class ResultRow

		} // class LoadDataForVatReturnSummary
	} // class CalculateVatReturnSummary
} // namespace
