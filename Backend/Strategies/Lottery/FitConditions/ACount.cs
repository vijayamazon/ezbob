namespace Ezbob.Backend.Strategies.Lottery.FitConditions {
	internal abstract class ACount : ABase {
		public override void Init(
			int actualLoanCount,
			int? configuredLoanCount,
			decimal actualLoanAmount,
			decimal? configuredLoanAmount
		) {
			ActualLoanCount = actualLoanCount;
			ConfiguredLoanCount = configuredLoanCount;
		} // Init

		public override bool Calculate() {
			return (null != ConfiguredLoanCount) && (0 <= ConfiguredLoanCount) && Fits();
		} // Calculate

		protected abstract bool Fits();

		protected virtual int ActualLoanCount { get; set; }
		protected virtual int? ConfiguredLoanCount { get; set; } 
	} // class MaxCount
} // namespace
