﻿namespace Ezbob.Utils.Formula.Boolean {
	public class Or : ABinaryBoolOperator {
		public Or(IBooleanFormula left, IBooleanFormula right) : base(left, right) {
		} // constructor

		protected override bool BoolOperator() {
			return LeftTerm.Calculate() || RightTerm.Calculate();
		} // BoolOperator
	} // class Or
} // namespace
