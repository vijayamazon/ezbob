using System;
using System.ServiceModel;

namespace EzService {
	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceAdmin {
		[OperationContract]
		ActionMetaData Shutdown();

		[OperationContract]
		ActionMetaData Nop(int nLengthInSeconds);

		[OperationContract(Name = "Kill")]
		ActionMetaData Terminate(string sActionID);

		[OperationContract]
		ActionMetaData Terminate(Guid sActionID);
	} // interface IEzServiceAdmin
} // namespace EzService
