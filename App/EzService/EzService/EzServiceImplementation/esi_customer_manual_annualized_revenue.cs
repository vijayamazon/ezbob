﻿namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.CustomerManualAnnualizedRevenue;

	partial class EzServiceImplementation {

		public CustomerManualAnnualizedRevenueActionResult GetCustomerManualAnnualizedRevenue(int nCustomerID) {
			GetCustomerManualAnnualizedRevenue oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, nCustomerID);

			return new CustomerManualAnnualizedRevenueActionResult {
				MetaData = oResult,
				Value = oInstance.Result,
			};
		} // GetCustomerManualAnnualizedRevenue

		public CustomerManualAnnualizedRevenueActionResult SetCustomerManualAnnualizedRevenue(int nCustomerID, decimal nRevenue, string sComment) {
			SetCustomerManualAnnualizedRevenue oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, nCustomerID, nRevenue, sComment);

			return new CustomerManualAnnualizedRevenueActionResult {
				MetaData = oResult,
				Value = oInstance.Result,
			};
		} // SetCustomerManualAnnualizedRevenue

	} // class EzServiceImplementation
} // namespace EzService
