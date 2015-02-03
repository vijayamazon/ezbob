namespace Ezbob.Utils.Formula.Boolean {
	public class Not : ABinaryBoolOperator {
		public Not(IBooleanFormula term) : base(term, term) {
		} // constructor

		protected override bool BoolOperator() {
			return !LeftTerm.Calculate();
		} // BoolOperator
	} // class Not
} // namespace
