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
			this.customerID = customerDetails.ID;
			this.customerFullName = customerDetails.FullName;
			this.customerIsAutoRejected = autoDecisionResponse.DecidedToReject;
			this.customerPropertyStatusDescription = customerDetails.PropertyStatusDescription;
			this.isOwnerOfMainAddress = customerDetails.IsOwnerOfMainAddress;
			this.isOwnerOfOtherProperties = customerDetails.IsOwnerOfOtherProperties;
			this.skipCheck = !newCreditLineOption.UpdateData();
		} // constructor

		public MainStrategyUpdateLandRegistryData(
			int customerID,
			string customerFullName,
			bool customerIsAutoRejected,
			string customerPropertyStatusDescription,
			bool isOwnerOfMainAddress,
			bool isOwnerOfOtherProperties,
			NewCreditLineOption newCreditLineOption
		) {
			this.customerID = customerID;
			this.customerFullName = customerFullName;
			this.customerIsAutoRejected = customerIsAutoRejected;
			this.customerPropertyStatusDescription = customerPropertyStatusDescription;
			this.isOwnerOfMainAddress = isOwnerOfMainAddress;
			this.isOwnerOfOtherProperties = isOwnerOfOtherProperties;
			this.skipCheck = !newCreditLineOption.UpdateData();
		} // constructor

		public void Execute() {
			if (this.skipCheck)
				return;

			var isHomeOwner = this.isOwnerOfMainAddress || this.isOwnerOfOtherProperties;

			string decisionName = this.customerIsAutoRejected ? "already rejected" : "not auto rejected";

			if (!this.customerIsAutoRejected && isHomeOwner) {
				Log.Debug(
					"Retrieving LandRegistry system decision: {0} residential status: {1}",
					decisionName,
					this.customerPropertyStatusDescription
				);

				try {
					UpdateLandRegistryData();
				} catch (Exception e) {
					Log.Error("Error while getting land registry data: {0}", e);
				} // try
			} else {
				Log.Info(
					"Not retrieving LandRegistry system decision: {0} residential status: {1}",
					decisionName,
					this.customerPropertyStatusDescription
				);
			} // if
		} // Execute

		private void UpdateLandRegistryData() {
			var customerAddressesHelper = new CustomerAddressHelper(this.customerID);
			customerAddressesHelper.Execute();

			foreach (CustomerAddressModel address in customerAddressesHelper.OwnedAddresses) {
				LandRegistryDataModel model = null;

				if (!string.IsNullOrEmpty(address.HouseName)) {
					model = LandRegistryEnquiry.Get(
						this.customerID,
						null,
						address.HouseName,
						null,
						null,
						address.PostCode
					);
				} else if (!string.IsNullOrEmpty(address.HouseNumber)) {
					model = LandRegistryEnquiry.Get(
						this.customerID,
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
						this.customerID,
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
					var lrr = new LandRegistryRes(this.customerID, model.Enquery.Titles[0].TitleNumber);
					lrr.PartialExecute();

					LandRegistry dbLandRegistry = lrr.LandRegistry;

					LandRegistryDataModel landRegistryDataModel = lrr.RawResult;

					if (landRegistryDataModel.ResponseType == LandRegistryResponseType.Success) {
						bool isOwnerAccordingToLandRegistry = LandRegistryRes.IsOwner(
							this.customerID,
							this.customerFullName,
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
						this.customerID
					);
				} // if
			} // for each
		} // UpdateLandRegistryData

		private static AConnection DB {
			get { return Library.Instance.DB; }
		} // Log

		private static ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log

		private readonly int customerID;
		private readonly string customerFullName;
		private readonly bool customerIsAutoRejected;
		private readonly string customerPropertyStatusDescription;
		private readonly bool isOwnerOfMainAddress;
		private readonly bool isOwnerOfOtherProperties;
		private readonly bool skipCheck;
	} // class MainStrategyUpdateLandRegistryData
} // namespace
