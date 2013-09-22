using System;

namespace Ezbob.ValueIntervals {
	#region class FreezeIntervalEdge

	public class FreezeIntervalEdge : AIntervalEdge<DateTime> {
		#region public

		#region constructor

		public FreezeIntervalEdge(DateTime? oDate, EdgeType nDefaultInfinityType) {
			if (oDate.HasValue) {
				Type = EdgeType.Finite;
				Value = oDate.Value;
			}
			else {
				Type = nDefaultInfinityType;
				Value = default(DateTime);
			} // if
		} // constructor

		#endregion constructor

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
	} // class FreezeIntervalEdge

	#endregion class FreezeIntervalEdge
} // namespace Ezbob.ValueIntervals
