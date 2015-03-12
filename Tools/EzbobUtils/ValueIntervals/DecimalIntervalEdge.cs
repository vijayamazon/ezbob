namespace Ezbob.ValueIntervals {
	public class DecimalIntervalEdge : ANumericIntervalEdge {
		public DecimalIntervalEdge(decimal edge) : base(edge) {} // constructor

		protected override decimal Epsilon { get { return 0.001M; } }
	} // class DecimalIntervalEdge
} // namespace
