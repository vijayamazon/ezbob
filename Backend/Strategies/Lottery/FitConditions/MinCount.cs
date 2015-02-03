namespace Ezbob.Backend.Strategies.Lottery.FitConditions {
	internal class MinCount : ACount {
		protected override bool Fits() {
			return ConfiguredLoanCount <= ActualLoanCount;
		} // Fits
	} // class MinCount
} // namespace
