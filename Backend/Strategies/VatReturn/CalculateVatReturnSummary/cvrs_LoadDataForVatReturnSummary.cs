namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class LoadDataForVatReturnSummary : AStoredProcedure {
			#region constructor

			public LoadDataForVatReturnSummary(int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
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
				#region DB output fields

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

				#endregion DB output fields

				#region property BoxNum

				public int BoxNum {
					get {
						if (!m_nBoxNum.HasValue)
							m_nBoxNum = VatReturnUtils.BoxNameToNum(BoxName);

						return m_nBoxNum.Value;
					} // get
				} // BoxNum

				private int? m_nBoxNum;

				#endregion property BoxNum
			} // class ResultRow

			#endregion class ResultRow
		} // class LoadDataForVatReturnSummary
	} // class CalculateVatReturnSummary
} // namespace
