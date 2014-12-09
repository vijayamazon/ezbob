namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.Misc;

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

		public BoolActionResult SendSms(int userId, int underwriterId, string phone, string content)
		{
			SendSms strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, underwriterId, userId, underwriterId, phone, content);
			return new BoolActionResult { MetaData = result, Value = strategyInstance.Result };
		} // SendSms

	} // class EzServiceImplementation
} // namespace EzService
