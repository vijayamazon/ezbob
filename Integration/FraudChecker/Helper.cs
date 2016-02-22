namespace FraudChecker {
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Fraud;

	public static class Helper {
		public static FraudDetection CreateDetection(
			string currentField,
			Customer currentCustomer,
			Customer internalCustomer,
			string compareField,
			FraudUser externalUser,
			string value
		) {
			return new FraudDetection {
				CompareField = compareField,
				CurrentCustomer = currentCustomer,
				InternalCustomer = internalCustomer,
				CurrentField = currentField,
				ExternalUser = externalUser,
				Value = value ?? string.Empty
			};
		} // CreateDetection
	} // class Helper
} // namespace
