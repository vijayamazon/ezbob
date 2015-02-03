namespace Ezbob.Utils.Formula.Boolean {
	public abstract class ABinaryBoolOperator : IBooleanFormula {
		public IBooleanFormula LeftTerm { get; private set; }
		public IBooleanFormula RightTerm { get; private set; }

		public virtual bool Calculate() {
			if ((LeftTerm == null) && (RightTerm == null))
				return false;

			if (LeftTerm == null)
				return RightTerm.Calculate();

			if (RightTerm == null)
				return LeftTerm.Calculate();

			return BoolOperator();
		} // Calculate

		protected ABinaryBoolOperator(IBooleanFormula left, IBooleanFormula right) {
			LeftTerm = left;
			RightTerm = right;
		} // constructor

		protected abstract bool BoolOperator();
	} // class ABinaryBoolOperator
} // namespace
