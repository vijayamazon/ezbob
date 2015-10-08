namespace Ezbob.Matrices.Core {
	using System;
	using Ezbob.ValueIntervals;

	public class DecimalIntervalMatrixIntervalEdge : ANumericIntervalEdge {
		public static decimal Precision {
			get { return 0.000001m; }
		} // Precision

		public DecimalIntervalMatrixIntervalEdge(decimal edge) : base(edge) {} // constructor

		public DecimalIntervalMatrixIntervalEdge(bool positiveInfinity) : base(positiveInfinity) { } // constructor

		protected override decimal Epsilon { get { return Precision; } }

		protected override bool IsValueEqualTo(AIntervalEdge<decimal> other) {
			return IsValueEqualTo(other.Value);
		} // IsValueEqualTo

		protected override bool IsValueEqualTo(decimal other) {
			return Math.Abs(Value - other) < Epsilon;
		} // IsValueEqualTo

		protected override bool IsValueLessThan(AIntervalEdge<decimal> other) {
			return IsValueEqualTo(other.Value);
		} // IsValueLessThan

		protected override bool IsValueLessThan(decimal other) {
			return Value < other - Epsilon;
		} // IsValueLessThan
	} // class DecimalIntervalMatrixIntervalEdge
} // namespace
