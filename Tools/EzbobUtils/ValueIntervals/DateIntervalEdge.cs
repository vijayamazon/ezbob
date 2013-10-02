using System;
using System.Globalization;

namespace Ezbob.ValueIntervals {
	#region class DateIntervalEdge

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

		#region method ToString

		public override string ToString() {
			return ToString("MMM dd yyyy", CultureInfo.InvariantCulture);
		} // ToString

		public virtual string ToString(string sFormat, CultureInfo ci) {
			switch (Type) {
			case EdgeType.NegativeInfinity:
				return "   never   ";

			case EdgeType.Finite:
				return Value.ToString(sFormat, ci);

			case EdgeType.PositiveInfinity:
				return "  forever  ";

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ToString

		#endregion method ToString

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

		#endregion protected
	} // class DateIntervalEdge

	#endregion class DateIntervalEdge
} // namespace Ezbob.ValueIntervals
