namespace Ezbob.ValueIntervals {
	using System;
	using System.Globalization;

	public abstract class ANumericIntervalEdge : AIntervalEdge<decimal> {
		public override AIntervalEdge<decimal> Previous() {
			switch (Type) {
			case EdgeType.NegativeInfinity:
				return (AIntervalEdge<decimal>)Activator.CreateInstance(GetType(), false);

			case EdgeType.PositiveInfinity:
				return (AIntervalEdge<decimal>)Activator.CreateInstance(GetType(), Decimal.MaxValue);

			default:
				return (AIntervalEdge<decimal>)Activator.CreateInstance(GetType(), Value - Epsilon);
			} // switch
		} // Previous

		public override AIntervalEdge<decimal> Next() {
			switch (Type) {
			case EdgeType.NegativeInfinity:
				return (AIntervalEdge<decimal>)Activator.CreateInstance(GetType(), Decimal.MinValue);

			case EdgeType.PositiveInfinity:
				return (AIntervalEdge<decimal>)Activator.CreateInstance(GetType(), true);

			default:
				return (AIntervalEdge<decimal>)Activator.CreateInstance(GetType(), Value + Epsilon);
			} // switch
		} // Next

		protected ANumericIntervalEdge(decimal edge) : base(EdgeType.Finite, edge) {
		} // constructor

		protected ANumericIntervalEdge(bool positiveInfinity)
			: base(positiveInfinity ? EdgeType.PositiveInfinity : EdgeType.NegativeInfinity, 0) {
		} // constructor

		protected abstract decimal Epsilon { get; }

		protected override bool IsValueEqualTo(AIntervalEdge<decimal> other) {
			return Value.CompareTo(other.Value) == 0;
		} // IsValueEqualTo

		protected override bool IsValueLessThan(AIntervalEdge<decimal> other) {
			return Value.CompareTo(other.Value) < 0;
		} // IsValueLessThan

		protected override string ValueToString() {
			return ValueToString(null, null);
		} // ValueToString

		protected override string ValueToString(string sFormat, CultureInfo ci) {
			return Value.ToString(sFormat ?? "N2", ci ?? CultureInfo.InvariantCulture);
		} // ValueToString

		protected override string InfinityToString(bool bPositive, CultureInfo oCultureInfo) {
			return bPositive ? " +∞ " : " -∞ ";
		} // InfinityToString
	} // class DateIntervalEdge
} // namespace
