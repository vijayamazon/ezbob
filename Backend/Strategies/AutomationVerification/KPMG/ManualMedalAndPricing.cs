namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	internal class ManualMedalAndPricing : AMedalAndPricing {
		protected override decimal SetupFeeAmount {
			get { return SetupFee; }
		} // SetupFeeAmount

		protected override decimal SetupFeePct {
			get { return Amount <= 0 ? 0 : SetupFee / Amount; }
		} // SetupFeePct
	} // ManualMedalAndPricing
} // namespace
