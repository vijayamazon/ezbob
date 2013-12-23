using System;
using System.ServiceModel;

namespace EzService {
	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceAdmin {
		[OperationContract]
		ActionMetaData Shutdown();

		[OperationContract]
		ActionMetaData Nop(int nLengthInSeconds);

		[OperationContract]
		ActionMetaData Terminate(Guid sActionID);

		[OperationContract]
		StringListActionResult ListActiveActions();

		[OperationContract]
		ActionMetaData WriteToLog(string sSeverity, string sMsg);
	} // interface IEzServiceAdmin
} // namespace EzService
