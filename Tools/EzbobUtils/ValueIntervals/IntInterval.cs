namespace Ezbob.ValueIntervals {
	public class IntInterval : TInterval<int> {
		public IntInterval(int a, int b) : base(new IntIntervalEdge(a), new IntIntervalEdge(b)) {
		} // constructor
	} // class IntInterval
} // namespace
