namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies;

	partial class EzServiceImplementation {
		public BoolActionResult GenerateMobileCode(string phone) {
			GenerateMobileCode strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, null, phone);
			return new BoolActionResult { MetaData = result, Value = strategyInstance.IsError };
		} // GenerateMobileCode

		public BoolActionResult ValidateMobileCode(string phone, string code) {
			ValidateMobileCode strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, null, phone, code);
			return new BoolActionResult { MetaData = result, Value = strategyInstance.IsValidatedSuccessfully() };
		} // ValidateMobileCode
	} // class EzServiceImplementation
} // namespace EzService
