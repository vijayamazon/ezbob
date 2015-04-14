namespace EzService.EzServiceImplementation {
	using System;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Backend.Strategies.AutomationVerification;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Backend.Strategies.Tasks;

	partial class EzServiceImplementation {
		public ActionMetaData MaamMedalAndPricing(int nCustomerCount, int nLastCheckedCashRequestID) {
			return Execute<MaamMedalAndPricing>(null, null, nCustomerCount, nLastCheckedCashRequestID);
		} // MaamMedalAndPricing

		public ActionMetaData VerifyReapproval(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyReapproval>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyReapproval

		public ActionMetaData VerifyApproval(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyApproval>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyApproval

		public ActionMetaData VerifyRerejection(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyRerejection>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyRerejection

		public ActionMetaData VerifyReject(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyReject>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyReject

		public ActionMetaData VerifyMedal(int topCount, int lastCheckedID, bool includeTest, DateTime? calculationTime) {
			return Execute<VerifyMedal>(null, null, topCount, lastCheckedID, includeTest, calculationTime);
		} // VerifyMedal

		public ActionMetaData SilentAutomation(int customerID, int underwriterID) {
			return Execute<SilentAutomation>(customerID, underwriterID, customerID);
		} // SilentAutomation

		public ActionMetaData TotalMaamMedalAndPricing() {
			return Execute<TotalMaamMedalAndPricing>(null, null);
		} // TotalMaamMedalAndPricing
	} // class EzServiceImplementation
} // namespace
