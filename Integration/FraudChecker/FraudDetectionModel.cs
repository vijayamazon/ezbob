namespace FraudChecker {
	public class FraudDetectionModel {
		public string CompareField { get; set; }
		public string CurrentCustomer { get; set; }
		public string InternalCustomer { get; set; }
		public string CurrentField { get; set; }
		public string ExternalUser { get; set; }
		public string Value { get; set; }

		public FraudDetectionModel() {
			CompareField = "";
			CurrentCustomer = null;
			InternalCustomer = null;
			CurrentField = "";
			ExternalUser = null;
			Value = "";
		} // constructor
	} // class FraudDetectionModel
} // namespace
