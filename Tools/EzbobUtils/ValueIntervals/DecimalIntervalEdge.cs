namespace Ezbob.ValueIntervals
{
	using System;
	using System.Globalization;

	public class DecimalIntervalEdge : AIntervalEdge<decimal>
	{

		public DecimalIntervalEdge(decimal edge)
		{
			Type = EdgeType.Finite;
			Value = edge;
		} // constructor

		public override AIntervalEdge<decimal> Previous()
		{
			return new DecimalIntervalEdge(Value+0.001M);
		} // Previous

		public override AIntervalEdge<decimal> Next()
		{
			return new DecimalIntervalEdge(Value + 0.001M);
		} // Next

		protected override bool IsValueEqualTo(AIntervalEdge<decimal> other)
		{
			return Value.CompareTo(other.Value) == 0;
		} // IsValueEqualTo

		protected override bool IsValueLessThan(AIntervalEdge<decimal> other)
		{
			return Value.CompareTo(other.Value) < 0;
		} // IsValueLessThan

		protected override string ValueToString()
		{
			return ValueToString(null, null);
		} // ValueToString

		protected override string ValueToString(string sFormat, CultureInfo ci)
		{
			return Value.ToString(sFormat ?? "N2", ci ?? CultureInfo.InvariantCulture);
		} // ValueToString

		protected override string InfinityToString(bool bPositive, CultureInfo oCultureInfo)
		{
			return bPositive ? " ∞ " : " -∞ ";
		} // InfinityToString

	} // class DateIntervalEdge
} // namespace
