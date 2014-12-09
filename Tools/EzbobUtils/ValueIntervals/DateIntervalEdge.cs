namespace Ezbob.ValueIntervals {
	using System;
	using System.Globalization;

	public class DateIntervalEdge : AIntervalEdge<DateTime> {

		public DateIntervalEdge(DateTime? oDate, EdgeType nDefaultInfinityType) {
			if (oDate.HasValue) {
				Type = EdgeType.Finite;
				Value = oDate.Value.Date;
			}
			else {
				Type = nDefaultInfinityType;
				Value = default(DateTime);
			} // if
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
			return Value.CompareTo(other.Value) == 0;
		} // IsValueEqualTo

		protected override bool IsValueLessThan(AIntervalEdge<DateTime> other) {
			return Value.CompareTo(other.Value) < 0;
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
