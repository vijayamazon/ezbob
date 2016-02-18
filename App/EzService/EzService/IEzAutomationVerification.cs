namespace EzService {
	using System;
	using System.ServiceModel;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzAutomationVerification {
		[OperationContract]
		ActionMetaData VerifyMedal(int topCount, int lastCheckedID, bool includeTest, DateTime? calculationTime);

		[OperationContract]
		ActionMetaData VerifyApproval(int nCustomerCount, int nLastCheckedCustomerID);

		[OperationContract]
		ActionMetaData VerifyReapproval(int nCustomerCount, int nLastCheckedCustomerID);

		[OperationContract]
		ActionMetaData VerifyReject(int nCustomerCount, int nLastCheckedCustomerID);

		[OperationContract]
		ActionMetaData VerifyRerejection(int nCustomerCount, int nLastCheckedCustomerID);

		[OperationContract]
		ActionMetaData SilentAutomation(int customerID, int underwriterID);
	} // interface IEzAutomationVerification
} // namespace
