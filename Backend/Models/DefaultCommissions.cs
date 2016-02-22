namespace Ezbob.Backend.Models {
	public class DefaultCommissions {
		public DefaultCommissions(decimal brokerCommission, decimal manualSetupFee) {
			BrokerCommission = brokerCommission;
			ManualSetupFee = manualSetupFee;
		} // constructor

		public decimal BrokerCommission { get; private set; }
		public decimal ManualSetupFee { get; private set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"broker commission = {0}, manual setup fee = {1}",
				BrokerCommission.ToString("P6"),
				ManualSetupFee.ToString("P6")
			);
		} // ToString
	} // class DefaultCommissions
} // namespace
