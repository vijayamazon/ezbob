namespace Ezbob.ValueIntervals {
	using System;

	public class IntIntervalEdge : AIntervalEdge<int> {
		public IntIntervalEdge(int val) : base(EdgeType.Finite, val) {
		} // constructor

		public IntIntervalEdge(bool positiveInfinity)
			: base(positiveInfinity ? EdgeType.PositiveInfinity : EdgeType.NegativeInfinity, 0)
		{
		} // constructor

		/// <summary>
		/// Create new instance: decrease current value by one.
		/// I.e. if value is DateTime.Now this method should .AddDays(-1)
		/// if value is 5 this method should set value to 4.
		/// </summary>
		public override AIntervalEdge<int> Previous() {
			switch (Type) {
			case EdgeType.NegativeInfinity:
				return new IntIntervalEdge(false);

			case EdgeType.PositiveInfinity:
				return new IntIntervalEdge(Int32.MaxValue);

			default:
				return new IntIntervalEdge(Value - 1);
			} // switch
		} // Previous

		/// <summary>
		/// Create new instance: increase current value by one.
		/// I.e. if value is DateTime.Now this method should .AddDays(1)
		/// if value is 5 this method should set value to 6.
		/// </summary>
		public override AIntervalEdge<int> Next() {
			switch (Type) {
			case EdgeType.NegativeInfinity:
				return new IntIntervalEdge(Int32.MinValue);

			case EdgeType.PositiveInfinity:
				return new IntIntervalEdge(true);

			default:
				return new IntIntervalEdge(Value + 1);
			} // switch
		} // Next

		protected override bool IsValueEqualTo(AIntervalEdge<int> other) {
			return Value == other.Value;
		} // IsValueEqualTo

		protected override bool IsValueEqualTo(int other) {
			return Value == other;
		} // IsValueEqualTo

		protected override bool IsValueLessThan(AIntervalEdge<int> other) {
			return Value < other.Value;
		} // IsValueLessThan

		protected override bool IsValueLessThan(int other) {
			return Value < other;
		} // IsValueLessThan
	} // class IntIntervalEdge
} // namespace
