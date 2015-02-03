namespace Ezbob.Backend.Strategies.Lottery.FitConditions {
	internal class MinAmount : AAmount {
		protected override bool Fits() {
			return ConfiguredLoanAmount <= ActualLoanAmount;
		} // Fits
	} // class MinAmount
} // namespace
