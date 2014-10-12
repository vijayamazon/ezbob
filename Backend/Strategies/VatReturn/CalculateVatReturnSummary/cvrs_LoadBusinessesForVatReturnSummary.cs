namespace EzBob.Backend.Strategies.VatReturn {
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class LoadBusinessesForVatReturnSummary : AStoredProcedure {
			public LoadBusinessesForVatReturnSummary(int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerMarketplaceID = nCustomerMarketplaceID;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerMarketplaceID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; }
		} // class LoadBusinessesForVatReturnSummary
	} // class CalculateVatReturnSummary
} // namespace
