namespace Ezbob.Matrices {
	using Ezbob.ValueIntervals;

	public class MatrixIntervalEdge : ANumericIntervalEdge {
		public MatrixIntervalEdge(decimal edge) : base(edge) {} // constructor

		protected override decimal Epsilon { get { return 0.000001M; } }
	} // class MatrixIntervalEdge
} // namespace
