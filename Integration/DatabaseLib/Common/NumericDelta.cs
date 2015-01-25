namespace EZBob.DatabaseLib.Common {

	public class NumericDelta<TNum> {

		public NumericDelta(TNum nStartValue) {
			StartValue = nStartValue;
			EndValue = nStartValue;
		} // constructor

		public TNum StartValue { get; private set; }

		public TNum EndValue { get; set; }

		public bool HasChanged {
			get { return !NotChanged; }
		} // HasChanged

		public bool NotChanged {
			get { return EndValue.Equals(StartValue); }
		} // NotChanged

		public override string ToString() {
			return string.Format("{0} --> {1}: {2}changed", StartValue, EndValue, HasChanged ? "" : "not ");
		} // ToString

	} // class NumericDelta

} // namespace EZBob.DatabaseLib.Model.Database.Loans
