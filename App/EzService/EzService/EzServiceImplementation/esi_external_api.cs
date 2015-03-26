namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Backend.Strategies.ExternalAPI;
	using Ezbob.Backend.Strategies.ExternalAPI.Alibaba;

	partial class EzServiceImplementation {

		public AlibabaAvailableCreditActionResult CustomerAvaliableCredit(int customerID, long aliMemberID) {

			CustomerAvaliableCredit instance;

			Log.Info("ESI CustomerAvaliableCredit: customerID: {0}, customerID: {1}", customerID, aliMemberID);

			ExecuteSync(out instance, customerID, null, customerID, aliMemberID);

			return new AlibabaAvailableCreditActionResult { Result = instance.Result };

		} // CustomerAvaliableCredit

		public ActionMetaData RequalifyCustomer(int customerID, long aliMemberID) {

			Log.Info("ESI RequalifyCustomer: customerID: {0}, customerID: {1}", customerID, aliMemberID);

			ActionMetaData amd = Execute<RequalifyCustomer>(customerID, null, customerID, aliMemberID);

			return amd;

		} //RequalifyCustomer

		public ActionMetaData SaveApiCall(ApiCallData data) {

			ActionMetaData amd = Execute<SaveApiCall>(data.CustomerID, null, data);

			return amd;

		} // SaveApiCall


	} // class EzServiceImplementation
} // namespace