namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class SaveVatReturnSummary : AStoredProcedure {
			public SaveVatReturnSummary(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (CustomerMarketplaceID > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; }

			[UsedImplicitly]
			public DateTime CreationDate {
				get { return DateTime.UtcNow; }
				set { }
			} // CreationDate

			[UsedImplicitly]
			public Guid CalculationID { get; set; }

			[UsedImplicitly]
			public IEnumerable<BusinessData> Totals { get; set; }

			[UsedImplicitly]
			public IEnumerable<BusinessDataEntry> Quarters { get; set; }
		} // SaveVatReturnSummary
	} // class CalculateVatReturnSummary
} // namespace
