namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using AutomationCalculator.Common;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using LandRegistryLib;

	internal class UpdateLandRegistryData : AOneExitStep {
		public UpdateLandRegistryData(
			string outerContextDescription,
			int customerID,
			string customerFullName,
			bool customerIsAutoRejected,
			AutoDecisionFlowTypes flowType,
			bool isOwnerOfMainAddress,
			bool isOwnerOfOtherProperties,
			NewCreditLineOption newCreditLineOption,
			bool customerIsTest,
			bool avoidAutoDecision
		) : base(outerContextDescription) {
			this.customerID = customerID;
			this.customerFullName = customerFullName;
			this.customerIsAutoRejected = customerIsAutoRejected;
			this.flowType = flowType;
			this.isOwnerOfMainAddress = isOwnerOfMainAddress;
			this.isOwnerOfOtherProperties = isOwnerOfOtherProperties;
			this.newCreditLineOption = newCreditLineOption;
			this.customerIsTest = customerIsTest;
			this.avoidAutoDecision = avoidAutoDecision;
		} // constructor

		protected override void ExecuteStep() {
			var isHomeOwner = this.isOwnerOfMainAddress || this.isOwnerOfOtherProperties;

			bool skipCheck =
				!this.newCreditLineOption.UpdateData() ||
				this.newCreditLineOption.AvoidAutoDecision() ||
				this.avoidAutoDecision ||
				this.customerIsTest ||
				!isHomeOwner ||
				this.customerIsAutoRejected ||
				(this.flowType != AutoDecisionFlowTypes.Internal);

			string decisionName = this.customerIsAutoRejected ? "already rejected" : "not auto rejected";

			Log.Msg(
				"{0}etrieving Land Registry data for {1}: "+
				"\n\tNew credit line option: {2}" +
				"\n\tAvoid auto decision: {3}" +
				"\n\tTest customer: {4}" +
				"\n\tOwner of the main address: {5}" +
				"\n\tOwner of other properties: {6}" +
				"\n\tSystem decision: {7}" +
				"\n\tFlow type: {8}",
				skipCheck ? "Not r" : "R",
				OuterContextDescription,
				this.newCreditLineOption,
				this.avoidAutoDecision ? "yes" : "no",
				this.customerIsTest ? "yes" : "no",
				this.isOwnerOfMainAddress ? "yes" : "no",
				this.isOwnerOfOtherProperties ? "yes" : "no",
				decisionName,
				this.flowType
			);

			if (skipCheck)
				return;

			try {
				QueryLandRegistry();
			} catch (Exception e) {
				Log.Alert(e, "Error querying Land Registry for {0}.", OuterContextDescription);
			} // try
		} // ExecuteStep

		private void QueryLandRegistry() {
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
		} // QueryLandRegistry

		private readonly int customerID;
		private readonly string customerFullName;
		private readonly bool customerIsAutoRejected;
		private readonly AutoDecisionFlowTypes flowType;
		private readonly bool isOwnerOfMainAddress;
		private readonly bool isOwnerOfOtherProperties;
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly bool customerIsTest;
		private readonly bool avoidAutoDecision;
	} // class UpdateLandRegistryData
} // namespace
