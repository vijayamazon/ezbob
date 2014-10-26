namespace EzService {
	using System;
	using System.ServiceModel;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceAdmin {
		[OperationContract]
		ActionMetaData Shutdown();

		[OperationContract]
		ActionMetaData Nop(int nLengthInSeconds, string sMsg);

		[OperationContract]
		ActionMetaData Noop();

		[OperationContract]
		ActionMetaData StressTestAction(int nLengthInSeconds, string sMsg);

		[OperationContract]
		ActionMetaData StressTestSync(int nLengthInSeconds, string sMsg);

		[OperationContract]
		ActionMetaData Terminate(Guid sActionID);

		[OperationContract]
		StringListActionResult ListActiveActions();

		[OperationContract]
		ActionMetaData WriteToLog(string sSeverity, string sMsg);
	} // interface IEzServiceAdmin
} // namespace EzService
