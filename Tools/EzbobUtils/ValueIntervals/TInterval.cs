using System;

using EdgeType = Ezbob.ValueIntervals.AIntervalEdge<System.DateTime>.EdgeType;

namespace Ezbob.ValueIntervals {
	#region class TInterval

	public class TInterval<TFinite> where TFinite : IComparable<TFinite> {
		#region public

		#region method Intersects

		public virtual bool Intersects(TInterval<TFinite> other) {
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

		#region property Left

		public virtual AIntervalEdge<TFinite> Left { get; private set; }

		#endregion property Left

		#region property Right

		public virtual AIntervalEdge<TFinite> Right { get; private set; }

		#endregion property Right

		#endregion public

		#region protected

		#region constructor

		protected TInterval(AIntervalEdge<TFinite> oLeft, AIntervalEdge<TFinite> oRight) {
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

		#endregion protected
	} // class TInterval

	#endregion class TInterval
} // namespace Ezbob.ValueIntervals
