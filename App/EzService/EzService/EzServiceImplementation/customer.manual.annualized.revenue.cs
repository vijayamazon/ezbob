namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.CustomerManualAnnualizedRevenue;

	partial class EzServiceImplementation {
		#region method GetCustomerManualAnnualizedRevenue

		public CustomerManualAnnualizedRevenueActionResult GetCustomerManualAnnualizedRevenue(int nCustomerID) {
			GetCustomerManualAnnualizedRevenue oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, nCustomerID);

			return new CustomerManualAnnualizedRevenueActionResult {
				MetaData = oResult,
				Value = oInstance.Result,
			};
		} // GetCustomerManualAnnualizedRevenue

		#endregion method GetCustomerManualAnnualizedRevenue

		#region method SetCustomerManualAnnualizedRevenue

		public CustomerManualAnnualizedRevenueActionResult SetCustomerManualAnnualizedRevenue(int nCustomerID, decimal nRevenue, string sComment) {
			SetCustomerManualAnnualizedRevenue oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, nCustomerID, nRevenue, sComment);

			return new CustomerManualAnnualizedRevenueActionResult {
				MetaData = oResult,
				Value = oInstance.Result,
			};
		} // SetCustomerManualAnnualizedRevenue

		#endregion method SetCustomerManualAnnualizedRevenue
	} // class EzServiceImplementation
} // namespace EzService
