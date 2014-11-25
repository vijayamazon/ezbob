namespace Ezbob.ValueIntervals {
	using System;

	public class TInterval<TFinite> : IComparable<TInterval<TFinite>> where TFinite : IComparable<TFinite> {
		#region public

		#region operator *

		public static TInterval<TFinite> operator *(TInterval<TFinite> a, TInterval<TFinite> b) {
			return ReferenceEquals(a, null) ? null : a.Intersection(b);
		} // operator *

		#endregion operator *

		#region operator -

		public static TDisjointIntervals<TFinite> operator -(TInterval<TFinite> a, TInterval<TFinite> b) {
			return ReferenceEquals(a, null) ? null : a.Difference(b);
		} // operator -

		#endregion operator -

		#region method Intersects

		public virtual bool Intersects(TInterval<TFinite> other) {
			if (ReferenceEquals(this, other))
				return true;

			if (ReferenceEquals(other, null))
				return false;

			return Contains(other.Left) || Contains(other.Right) || other.Contains(this);
		} // Intersects

		#endregion method Intersects

		#region method Contains

		public virtual bool Contains(AIntervalEdge<TFinite> p) {
			return (Left <= p) && (p <= Right);
		} // Contains

		public virtual bool Contains(TInterval<TFinite> other) {
			return (Left <= other.Left) && (other.Right <= Right);
		} // Contains

		#endregion method Contains

		#region method ToString

		public override string ToString() { return string.Format("[ {0} - {1} ]", Left, Right); } // ToString

		#endregion method ToString

		#region method CompareTo

		public int CompareTo(TInterval<TFinite> other) {
			if (ReferenceEquals(other, null))
				throw new ArgumentNullException();

			return Left.CompareTo(other.Left);
		} // CompareTo

		#endregion method CompareTo

		#region property Left

		public virtual AIntervalEdge<TFinite> Left { get; private set; }

		#endregion property Left

		#region property Right

		public virtual AIntervalEdge<TFinite> Right { get; private set; }

		#endregion property Right

		#endregion public

		#region protected

		#region constructor

		public TInterval(AIntervalEdge<TFinite> oLeft, AIntervalEdge<TFinite> oRight) {
			if (ReferenceEquals(oLeft, null) || ReferenceEquals(oRight, null))
				throw new ArgumentNullException();

			Left = oLeft;
			Right = oRight;
		} // constructor

		protected TInterval(TInterval<TFinite> other) : this(other.Left, other.Right) {} // constructor

		protected TInterval(Tuple<AIntervalEdge<TFinite>, AIntervalEdge<TFinite>> oEdges) : this(oEdges.Item1, oEdges.Item2) {
		} // constructor

		#endregion constructor

		#region method Intersection

		protected virtual TInterval<TFinite> Intersection(TInterval<TFinite> other) {
			if (!Intersects(other))
				return null;

			return new TInterval<TFinite>(Left.Max(other.Left), Right.Min(other.Right));
		} // Intersection

		#endregion method Intersection

		#region method Difference

		protected virtual TDisjointIntervals<TFinite> Difference(TInterval<TFinite> other) {
			if (ReferenceEquals(this, other))
				return null;

			if (!Intersects(other))
				return new TDisjointIntervals<TFinite>(new TInterval<TFinite>(this));

			if (other.Contains(this))
				return null;

			if (Contains(other)) {
				return new TDisjointIntervals<TFinite>(
					new TInterval<TFinite>(Left, other.Left.Previous()),
					new TInterval<TFinite>(other.Right.Next(), Right)
				);
			} // if

			if (Contains(other.Left)) {
				return new TDisjointIntervals<TFinite>(
					new TInterval<TFinite>(Left, other.Left.Previous())
				);
			} // if

			return new TDisjointIntervals<TFinite>(
				new TInterval<TFinite>(other.Right.Next(), Right)
			);
		} // Difference

		#endregion method Difference

		#endregion protected
	} // class TInterval
} // namespace
