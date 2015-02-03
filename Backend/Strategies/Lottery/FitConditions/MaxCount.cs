namespace Ezbob.Backend.Strategies.Lottery.FitConditions {
	internal class MaxCount : ACount {
		protected override bool Fits() {
			return (0 < ActualLoanCount) && (ActualLoanCount <= ConfiguredLoanCount);
		} // Fits
	} // class MaxCount
} // namespace
