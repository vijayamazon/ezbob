namespace Ezbob.Backend.Strategies.Lottery.FitConditions {
	using Ezbob.Utils.Formula.Boolean;

	public interface IFittable : IBooleanFormula {
		void Init(
			int actualLoanCount,
			int? configuredLoanCount,
			decimal actualLoanAmount,
			decimal? configuredLoanLoanAmount
		);
	} // interface IFittable
} // namespace
