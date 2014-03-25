namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies;

	partial class EzServiceImplementation {
		public ActionMetaData MainStrategy1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison) {
			return Execute(customerId, underwriterId, typeof(MainStrategy), customerId, newCreditLine, avoidAutoDescison);
		} // MainStrategy1

		public ActionMetaData MainStrategy2(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison, bool isUnderwriterForced) {
			return Execute(customerId, underwriterId, typeof(MainStrategy), customerId, newCreditLine, avoidAutoDescison, isUnderwriterForced);
		} // MainStrategy2

		public ActionMetaData MainStrategy3(int underwriterId, int customerId, int checkType, string houseNumber, string houseName, string street, string district, string town, string county, string postcode, string bankAccount, string sortCode, int avoidAutoDescison) {
			return Execute(customerId, underwriterId, typeof(MainStrategy), customerId, checkType, houseNumber, houseName, street, district, town, county, postcode, bankAccount, sortCode, avoidAutoDescison);
		} // MainStrategy3

		public ActionMetaData MainStrategySync1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison) {
			return ExecuteSync<MainStrategy>(customerId, underwriterId, customerId, newCreditLine, avoidAutoDescison);
		} // MainStrategySync1
	} // class EzServiceImplementation
} // namespace EzService
