namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	internal class AutoMedalAndPricing : AMedalAndPricing {
		protected override decimal SetupFeeAmount {
			get { return SetupFee * Amount; }
		} // SetupFeeAmount

		protected override decimal SetupFeePct {
			get { return SetupFee; }
		} // SetupFeePct
	} // AutoMedalAndPricing
} // namespace
