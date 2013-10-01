using System;

namespace Ezbob.ValueIntervals {
	#region class AIntervalEdge

	public abstract class AIntervalEdge<TFinite> : IComparable<AIntervalEdge<TFinite>> where TFinite: IComparable<TFinite> {
		#region public

		#region comparison operators

		#region operator ==

		public static bool operator ==(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			if (ReferenceEquals(a, b))
				return true;

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;

			if (a.Type == b.Type)
				return (a.Type != EdgeType.Finite) || a.IsValueEqualTo(b);

			return false;
		} // operator ==

		#endregion operator ==

		#region operator !=

		public static bool operator !=(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			return !(a == b);
		} // operator !=

		#endregion operator !=

		#region operator <

		public static bool operator <(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			if (ReferenceEquals(a, b))
				return false;

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				throw new ValueIntervalException("One of operands is null.");

			if ((a.Type == EdgeType.NegativeInfinity) && (b.Type != EdgeType.NegativeInfinity))
				return true;

			if ((a.Type != EdgeType.PositiveInfinity) && (b.Type == EdgeType.PositiveInfinity))
				return true;

			if ((a.Type == EdgeType.Finite) && (b.Type == EdgeType.Finite))
				return a.IsValueLessThan(b);

			return false;
		} // operator <

		#endregion operator <

		#region operator <=

		public static bool operator <=(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			return (a == b) || (a < b);
		} // operator <=

		#endregion operator <=

		#region operator >=

		public static bool operator >=(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			return !(a < b);
		} // operator >=

		#endregion operator >=

		#region operator >

		public static bool operator >(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			return !(a <= b);
		} // operator >

		#endregion operator >

		#endregion comparison operators

		#region method Min

		public virtual AIntervalEdge<TFinite> Min(AIntervalEdge<TFinite> other) {
			if (ReferenceEquals(other, null))
				throw new ArgumentNullException();

			return this < other ? this : other;
		} // Min

		#endregion method Min

		#region method Max

		public virtual AIntervalEdge<TFinite> Max(AIntervalEdge<TFinite> other) {
			if (ReferenceEquals(other, null))
				throw new ArgumentNullException();

			return this > other ? this : other;
		} // Max

		#endregion method Max

		#region enum EdgeType

		public enum EdgeType {
			NegativeInfinity,
			Finite,
			PositiveInfinity,
		} // EdgeType

		#endregion enum EdgeType

		#region property Type

		public virtual EdgeType Type { get; protected set; }

		#endregion property Type

		#region property IsFinite

		public virtual bool IsFinite { get { return Type == EdgeType.Finite; } } // IsFinite

		#endregion property IsFinite

		#region property Value

		public virtual TFinite Value { get; protected set; }

		#endregion property Value

		#region method Equals

		public override bool Equals(object obj) {
			return this == (AIntervalEdge<TFinite>)obj;
		}// Equals

		#endregion method Equals

		#region method GetHashCode

		public override int GetHashCode() {
			switch (Type) {
			case EdgeType.NegativeInfinity:
			case EdgeType.PositiveInfinity:
				return Type.GetHashCode();

			case EdgeType.Finite:
				return Value.GetHashCode();

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // GetHashCode

		#endregion method GetHashCode

		#region method CompareTo

		public virtual int CompareTo(AIntervalEdge<TFinite> other) {
			if (other == null)
				throw new ValueIntervalException("Cannot compare to null.");

			if (Type == other.Type)
				return Type == EdgeType.Finite ? Value.CompareTo(other.Value) : 0;

			switch (Type) {
			case EdgeType.NegativeInfinity:
				return -1;

			case EdgeType.Finite:
				return other.Type == EdgeType.NegativeInfinity ? 1 : -1;

			case EdgeType.PositiveInfinity:
				return 1;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // CompareTo

		#endregion method CompareTo

		#endregion public

		#region protected

		#region constructor

		protected AIntervalEdge(EdgeType nType = default(EdgeType), TFinite oValue = default(TFinite)) {
			Type = nType;
			Value = oValue;
		} // constructor

		#endregion constructor

		protected abstract bool IsValueEqualTo(AIntervalEdge<TFinite> other);

		protected abstract bool IsValueLessThan(AIntervalEdge<TFinite> other);

		#endregion protected
	} // class AIntervalEdge

	#endregion class AIntervalEdge
} // namespace Ezbob.ValueIntervals
