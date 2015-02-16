namespace Ezbob.ValueIntervals {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class TDisjointIntervals<TFinite> : SortedSet<TInterval<TFinite>> where TFinite : IComparable<TFinite>  {
		public static TDisjointIntervals<TFinite> operator +(
			TDisjointIntervals<TFinite> oSet,
			TInterval<TFinite> oInterval
		) {
			return ReferenceEquals(oSet, null) ? null : oSet.Add(oInterval);
		} // operator +

		public static TDisjointIntervals<TFinite> operator +(
			TDisjointIntervals<TFinite> oSet,
			TDisjointIntervals<TFinite> oOther
		) {
			return ReferenceEquals(oSet, null) ? null : oSet.Add(oOther);
		} // operator +

		public TDisjointIntervals() {} // constructor

		public TDisjointIntervals(TInterval<TFinite> a) : this() {
			Add(a);
		} // constructor

		public TDisjointIntervals(TInterval<TFinite> a, TInterval<TFinite> b) : this(a) {
			Add(b);
		} // constructor

		public new virtual TDisjointIntervals<TFinite> Add(TInterval<TFinite> oInterval) {
			if (oInterval == null)
				return this;

			if (this.Any(i => i.Intersects(oInterval)))
				throw new ArgumentException("Interval to add intersects with an interval in the set.");

			base.Add(oInterval);

			return this;
		} // Add

		public virtual TDisjointIntervals<TFinite> Add(TDisjointIntervals<TFinite> oSet) {
			if (oSet == null)
				return this;

			foreach (TInterval<TFinite> oInterval in oSet) {
				try {
					this.Add(oInterval);
				}
				catch {
					// Silently ignore.
				} // try
			} // foreach

			return this;
		} // Add

		public override string ToString() {
			var os = new StringBuilder();

			os.Append("{");

			foreach (TInterval<TFinite> i in this)
				os.AppendFormat(" {0}", i);

			os.Append(" }");

			return os.ToString();
		} // ToString
	} // class TDisjointIntervals
} // namespace
