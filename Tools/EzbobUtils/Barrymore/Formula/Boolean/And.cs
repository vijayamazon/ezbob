namespace Ezbob.Utils.Formula.Boolean {
	public class And : ABinaryBoolOperator {
		public And(IBooleanFormula left, IBooleanFormula right) : base(left, right) {
		} // constructor

		protected override bool BoolOperator() {
			return LeftTerm.Calculate() && RightTerm.Calculate();
		} // BoolOperator
	} // class And
} // namespace
