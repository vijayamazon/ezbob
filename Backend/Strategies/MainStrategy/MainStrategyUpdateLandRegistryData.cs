namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using LandRegistryLib;

	class MainStrategyUpdateLandRegistryData {
		public MainStrategyUpdateLandRegistryData(
			CustomerDetails customerDetails,
			NewCreditLineOption newCreditLineOption,
			AutoDecisionResponse autoDecisionResponse
		) {
			this.customerDetails = customerDetails;
			this.newCreditLineOption = newCreditLineOption;
			this.autoDecisionResponse = autoDecisionResponse;
		} // constructor

		public void Execute() {
			bool bSkip =
				this.newCreditLineOption == NewCreditLineOption.SkipEverything ||
				this.newCreditLineOption == NewCreditLineOption.SkipEverythingAndApplyAutoRules;

			if (bSkip)
				return;

			var isHomeOwner = this.customerDetails.IsOwnerOfMainAddress || this.customerDetails.IsOwnerOfOtherProperties;

			if (!this.autoDecisionResponse.DecidedToReject && isHomeOwner) {
				Log.Debug(
					"Retrieving LandRegistry system decision: {0} residential status: {1}",
					this.autoDecisionResponse.DecisionName,
					this.customerDetails.PropertyStatusDescription
				);

				try {
					UpdateLandRegistryData();
				} catch (Exception e) {
					Log.Error("Error while getting land registry data: {0}", e);
				} // try
			} else {
				Log.Info(
					"Not retrieving LandRegistry system decision: {0} residential status: {1}",
					this.autoDecisionResponse.DecisionName,
					this.customerDetails.PropertyStatusDescription
				);
			} // if
		} // Execute

		private void UpdateLandRegistryData() {
			var customerAddressesHelper = new CustomerAddressHelper(CustomerID);
			customerAddressesHelper.Execute();

			foreach (CustomerAddressModel address in customerAddressesHelper.OwnedAddresses) {
				LandRegistryDataModel model = null;

				if (!string.IsNullOrEmpty(address.HouseName)) {
					model = LandRegistryEnquiry.Get(
						CustomerID,
						null,
						address.HouseName,
						null,
						null,
						address.PostCode
					);
				} else if (!string.IsNullOrEmpty(address.HouseNumber)) {
					model = LandRegistryEnquiry.Get(
						CustomerID,
						address.HouseNumber,
						null,
						null,
						null,
						address.PostCode
					);
				} else if (
					!string.IsNullOrEmpty(address.FlatOrApartmentNumber) &&
					string.IsNullOrEmpty(address.HouseNumber)
				) {
					model = LandRegistryEnquiry.Get(
						CustomerID,
						address.FlatOrApartmentNumber,
						null,
						null,
						null,
						address.PostCode
					);
				} // if

				bool doLandRegistry =
					(model != null) &&
					(model.Enquery != null) &&
					(model.ResponseType == LandRegistryResponseType.Success) &&
					(model.Enquery.Titles != null) &&
					(model.Enquery.Titles.Count == 1);

				if (doLandRegistry) {
					var lrr = new LandRegistryRes(CustomerID, model.Enquery.Titles[0].TitleNumber);
					lrr.PartialExecute();

					LandRegistry dbLandRegistry = lrr.LandRegistry;

					LandRegistryDataModel landRegistryDataModel = lrr.RawResult;

					if (landRegistryDataModel.ResponseType == LandRegistryResponseType.Success) {
						bool isOwnerAccordingToLandRegistry = LandRegistryRes.IsOwner(
							CustomerID,
							this.customerDetails.FullName,
							landRegistryDataModel.Response,
							landRegistryDataModel.Res.TitleNumber
						);

						DB.ExecuteNonQuery(
							"AttachCustomerAddrToLandRegistryAddr",
							CommandSpecies.StoredProcedure,
							new QueryParameter("@LandRegistryAddressID", dbLandRegistry.Id),
							new QueryParameter("@CustomerAddressID", address.AddressId),
							new QueryParameter("@IsOwnerAccordingToLandRegistry", isOwnerAccordingToLandRegistry)
						);
					} // if
				} else {
					int num = 0;

					if (model != null && model.Enquery != null && model.Enquery.Titles != null)
						num = model.Enquery.Titles.Count;

					Log.Warn(
						"No land registry retrieved for customer id: {5}," +
						"house name: {0}, house number: {1}, flat number: {2}, postcode: {3}, # of inquiries {4}",
						address.HouseName,
						address.HouseNumber,
						address.FlatOrApartmentNumber,
						address.PostCode,
						num,
						CustomerID
					);
				} // if
			} // for each
		} // UpdateLandRegistryData

		private int CustomerID {
			get { return this.customerDetails.ID; }
		} // CustomerID

		private static AConnection DB {
			get { return Library.Instance.DB; }
		} // Log

		private static ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log

		private readonly CustomerDetails customerDetails;
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly AutoDecisionResponse autoDecisionResponse;
	} // class MainStrategyUpdateLandRegistryData
} // namespace
