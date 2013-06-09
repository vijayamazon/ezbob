namespace EZBob.DatabaseLib.Model.Database.Loans {
	#region class NumericDelta

	public class NumericDelta<TNum> {
		#region public

		#region constructor

		public NumericDelta(TNum nStartValue) {
			StartValue = nStartValue;
			EndValue = nStartValue;
		} // constructor

		#endregion constructor

		#region property StartValue

		public TNum StartValue { get; private set; }

		#endregion property StartValue

		#region property EndValue

		public TNum EndValue { get; set; }

		#endregion property EndValue

		#region property HasChanged

		public bool HasChanged {
			get { return !NotChanged; }
		} // HasChanged

		#endregion property HasChanged

		#region property NotChanged

		public bool NotChanged {
			get { return EndValue.Equals(StartValue); }
		} // NotChanged

		#endregion property NotChanged

		#region method ToString

		public override string ToString() {
			return string.Format("{0} --> {1}: {2}changed", StartValue, EndValue, HasChanged ? "" : "not ");
		} // ToString

		#endregion method ToString

		#endregion public
	} // class NumericDelta

	#endregion class NumericDelta
} // namespace EZBob.DatabaseLib.Model.Database.Loans
