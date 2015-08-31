namespace Ezbob.ValueIntervals {
	using System;
	using System.Globalization;

	public class DateIntervalEdge : AIntervalEdge<DateTime> {
		public DateIntervalEdge(DateTime? oDate, EdgeType nDefaultInfinityType) : base(
			oDate.HasValue ? EdgeType.Finite : nDefaultInfinityType,
			oDate.HasValue ? oDate.Value.Date : default (DateTime)
		) {
		} // constructor

		public override AIntervalEdge<DateTime> Previous() {
			if (!IsFinite)
				return new DateIntervalEdge(Value, Type);

			return new DateIntervalEdge(Value.AddDays(-1), EdgeType.Finite);
		} // Previous

		public override AIntervalEdge<DateTime> Next() {
			if (!IsFinite)
				return new DateIntervalEdge(Value, Type);

			return new DateIntervalEdge(Value.AddDays(1), EdgeType.Finite);
		} // Next

		protected override bool IsValueEqualTo(AIntervalEdge<DateTime> other) {
			return IsValueEqualTo(other.Value);
		} // IsValueEqualTo

		protected override bool IsValueEqualTo(DateTime other) {
			return Value.CompareTo(other) == 0;
		} // IsValueEqualTo

		protected override bool IsValueLessThan(AIntervalEdge<DateTime> other) {
			return IsValueLessThan(other.Value);
		} // IsValueLessThan

		protected override bool IsValueLessThan(DateTime other) {
			return Value.CompareTo(other) < 0;
		} // IsValueLessThan

		protected override string ValueToString() {
			return ValueToString(null, null);
		} // ValueToString

		protected override string ValueToString(string sFormat, CultureInfo ci) {
			return Value.ToString(sFormat ?? "MMM dd yyyy", ci ?? CultureInfo.InvariantCulture);
		} // ValueToString

		protected override string InfinityToString(bool bPositive, CultureInfo oCultureInfo) {
			return bPositive ? "  forever  " : "   never   ";
		} // InfinityToString
	} // class DateIntervalEdge
} // namespace
