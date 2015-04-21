namespace EzBobTest {
	using NUnit.Framework;
	using PaymentServices.Calculators;

	[TestFixture]
	class TestSetupFeeCalcLegacy : BaseTestFixtue {
		[Test]
		public void SimpleTest() {
			const int useSetupFee = 1;
			const bool useBrokerSetupFee = false;
			const int approvedAmount = 84000;
			int? manualSetupFeeAmount = 4000;
			decimal? manualSetupFeePercent = null;

			var calc = new SetupFeeCalculatorLegacy(
				useSetupFee == 1,
				useBrokerSetupFee,
				manualSetupFeeAmount,
				manualSetupFeePercent
			);

			decimal setupFeeAmount = calc.Calculate(approvedAmount, true);

			var setupFeePct = approvedAmount <= 0 ? 0 : setupFeeAmount / approvedAmount;
		} // SimpleTest
	} // class TestSetupFeeCalcLegacy
} // namespace
