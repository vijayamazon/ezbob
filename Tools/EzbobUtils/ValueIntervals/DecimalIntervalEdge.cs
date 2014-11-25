namespace Ezbob.ValueIntervals
{
	using System;
	using System.Globalization;

	public class DecimalIntervalEdge : AIntervalEdge<decimal>
	{
		#region public

		#region constructor

		public DecimalIntervalEdge(decimal edge)
		{
			Type = EdgeType.Finite;
			Value = edge;
		} // constructor

		#endregion constructor

		#region method Previous

		public override AIntervalEdge<decimal> Previous()
		{
			return new DecimalIntervalEdge(Value+0.001M);
		} // Previous

		#endregion method Previous

		#region method Next

		public override AIntervalEdge<decimal> Next()
		{
			return new DecimalIntervalEdge(Value + 0.001M);
		} // Next

		#endregion method Next

		#endregion public

		#region protected

		#region method IsValueEqualTo

		protected override bool IsValueEqualTo(AIntervalEdge<decimal> other)
		{
			return Value.CompareTo(other.Value) == 0;
		} // IsValueEqualTo

		#endregion method IsValueEqualTo

		#region method IsValueLessThan

		protected override bool IsValueLessThan(AIntervalEdge<decimal> other)
		{
			return Value.CompareTo(other.Value) < 0;
		} // IsValueLessThan

		#endregion method IsValueLessThan

		#region method ValueToString

		protected override string ValueToString()
		{
			return ValueToString(null, null);
		} // ValueToString

		protected override string ValueToString(string sFormat, CultureInfo ci)
		{
			return Value.ToString(sFormat ?? "N2", ci ?? CultureInfo.InvariantCulture);
		} // ValueToString

		#endregion method ValueToString

		#region method InfinityToString

		protected override string InfinityToString(bool bPositive, CultureInfo oCultureInfo)
		{
			return bPositive ? " ∞ " : " -∞ ";
		} // InfinityToString

		#endregion method InfinityToString

		#endregion protected
	} // class DateIntervalEdge
} // namespace
