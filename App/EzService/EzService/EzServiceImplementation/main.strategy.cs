namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.MainStrategy;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {
		public ActionMetaData MainStrategy1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison) {
			return Execute<MainStrategy>(customerId, underwriterId, customerId, newCreditLine, avoidAutoDescison);
		} // MainStrategy1

		public ActionMetaData MainStrategy2(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison, bool isUnderwriterForced) {
			return Execute<MainStrategy>(customerId, underwriterId, customerId, newCreditLine, avoidAutoDescison, isUnderwriterForced);
		} // MainStrategy2

		public ActionMetaData MainStrategySync1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison) {
			return ExecuteSync<MainStrategy>(customerId, underwriterId, customerId, newCreditLine, avoidAutoDescison);
		} // MainStrategySync1
	} // class EzServiceImplementation
} // namespace EzService
