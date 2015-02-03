namespace Ezbob.Backend.Strategies.Lottery.FitConditions {
	internal class MaxAmount : AAmount {
		protected override bool Fits() {
			return (0 < ActualLoanAmount) && (ActualLoanAmount <= ConfiguredLoanAmount);
		} // Fits
	} // class MaxAmount
} // namespace
