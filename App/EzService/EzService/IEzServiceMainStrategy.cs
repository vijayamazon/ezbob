namespace EzService {
	using System.ServiceModel;
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib.Model.Database;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceMainStrategy {
		[OperationContract]
		ActionMetaData MainStrategyAsync(
			int uderwriterId,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			long? cashRequestID,
			CashRequestOriginator? cashRequestOriginator
		);

		[OperationContract]
		ActionMetaData MainStrategySync(
			int underwriterId,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			long? cashRequestID,
			CashRequestOriginator? cashRequestOriginator
		);
	} // class IEzServiceMainStrategy
} // namespace
