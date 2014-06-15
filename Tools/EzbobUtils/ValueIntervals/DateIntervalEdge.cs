namespace Ezbob.ValueIntervals {
	using System;
	using System.Globalization;

	public class DateIntervalEdge : AIntervalEdge<DateTime> {
		#region public

		#region constructor

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

		#endregion constructor

		#region method Previous

		public override AIntervalEdge<DateTime> Previous() {
			if (!IsFinite)
				return new DateIntervalEdge(Value, Type);

			return new DateIntervalEdge(Value.AddDays(-1), EdgeType.Finite);
		} // Previous

		#endregion method Previous

		#region method Next

		public override AIntervalEdge<DateTime> Next() {
			if (!IsFinite)
				return new DateIntervalEdge(Value, Type);

			return new DateIntervalEdge(Value.AddDays(1), EdgeType.Finite);
		} // Next

		#endregion method Next

		#endregion public

		#region protected

		#region method IsValueEqualTo

		protected override bool IsValueEqualTo(AIntervalEdge<DateTime> other) {
			return Value.CompareTo(other.Value) == 0;
		} // IsValueEqualTo

		#endregion method IsValueEqualTo

		#region method IsValueLessThan

		protected override bool IsValueLessThan(AIntervalEdge<DateTime> other) {
			return Value.CompareTo(other.Value) < 0;
		} // IsValueLessThan

		#endregion method IsValueLessThan

		#region method ValueToString

		protected override string ValueToString() {
			return ValueToString(null, null);
		} // ValueToString

		protected override string ValueToString(string sFormat, CultureInfo ci) {
			return Value.ToString(sFormat ?? "MMM dd yyyy", ci ?? CultureInfo.InvariantCulture);
		} // ValueToString

		#endregion method ValueToString

		#region method InfinityToString

		protected override string InfinityToString(bool bPositive, CultureInfo oCultureInfo) {
			return bPositive ? "  forever  " : "   never   ";
		} // InfinityToString

		#endregion method InfinityToString

		#endregion protected
	} // class DateIntervalEdge
} // namespace
