namespace Ezbob.ValueIntervals {
	using System;
	using System.Globalization;

	public abstract class AIntervalEdge<TFinite> :
		IComparable<AIntervalEdge<TFinite>>,
		IOrdinal<AIntervalEdge<TFinite>>
		where TFinite: IComparable<TFinite>
	{
		public static bool operator ==(AIntervalEdge<TFinite> a, TFinite b) {
			if (ReferenceEquals(a, b))
				return true;

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;

			return a.IsFinite && a.IsValueEqualTo(b);
		} // operator ==

		public static bool operator !=(AIntervalEdge<TFinite> a, TFinite b) {
			return !(a == b);
		} // operator !=

		public static bool operator <(AIntervalEdge<TFinite> a, TFinite b) {
			if (ReferenceEquals(a, b))
				return false;

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;

			if (a.IsFinite)
				return a.IsValueLessThan(b);

			if (a.Type == EdgeType.NegativeInfinity)
				return true;

			return false;
		} // operator <

		public static bool operator <=(AIntervalEdge<TFinite> a, TFinite b) {
			return (a == b) || (a < b);
		} // operator <=

		public static bool operator >=(AIntervalEdge<TFinite> a, TFinite b) {
			return !(a < b);
		} // operator >=

		public static bool operator >(AIntervalEdge<TFinite> a, TFinite b) {
			return !(a <= b);
		} // operator >

		public static bool operator ==(TFinite a, AIntervalEdge<TFinite> b) {
			return b == a;
		} // operator ==

		public static bool operator !=(TFinite a, AIntervalEdge<TFinite> b) {
			return !(a == b);
		} // operator !=

		public static bool operator <(TFinite a, AIntervalEdge<TFinite> b) {
			return b > a;
		} // operator <

		public static bool operator <=(TFinite a, AIntervalEdge<TFinite> b) {
			return (a == b) || (a < b);
		} // operator <=

		public static bool operator >=(TFinite a, AIntervalEdge<TFinite> b) {
			return !(a < b);
		} // operator >=

		public static bool operator >(TFinite a, AIntervalEdge<TFinite> b) {
			return !(a <= b);
		} // operator >

		public static bool operator ==(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			if (ReferenceEquals(a, b))
				return true;

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;

			if (a.Type == b.Type)
				return (a.Type != EdgeType.Finite) || a.IsValueEqualTo(b);

			return false;
		} // operator ==

		public static bool operator !=(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			return !(a == b);
		} // operator !=

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

		public static bool operator <=(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			return (a == b) || (a < b);
		} // operator <=

		public static bool operator >=(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			return !(a < b);
		} // operator >=

		public static bool operator >(AIntervalEdge<TFinite> a, AIntervalEdge<TFinite> b) {
			return !(a <= b);
		} // operator >

		public virtual AIntervalEdge<TFinite> Min(AIntervalEdge<TFinite> other) {
			if (ReferenceEquals(other, null))
				throw new ArgumentNullException();

			return this < other ? this : other;
		} // Min

		public virtual AIntervalEdge<TFinite> Max(AIntervalEdge<TFinite> other) {
			if (ReferenceEquals(other, null))
				throw new ArgumentNullException();

			return this > other ? this : other;
		} // Max

		public enum EdgeType {
			NegativeInfinity,
			Finite,
			PositiveInfinity,
		} // EdgeType

		public virtual EdgeType Type { get; private set; }

		public virtual bool IsFinite { get { return Type == EdgeType.Finite; } } // IsFinite

		public virtual TFinite Value { get; private set; }

		public override bool Equals(object obj) {
			return this == (AIntervalEdge<TFinite>)obj;
		}// Equals

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

		public virtual int CompareTo(AIntervalEdge<TFinite> other) {
			if (other == null)
				throw new ArgumentNullException();

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

		public override string ToString() {
			return ToString(null, CultureInfo.InvariantCulture);
		} // ToString

		public virtual string ToString(string sFormat, CultureInfo ci) {
			switch (Type) {
			case EdgeType.NegativeInfinity:
				return InfinityToString(false, ci);

			case EdgeType.Finite:
				return ValueToString(sFormat, ci);

			case EdgeType.PositiveInfinity:
				return InfinityToString(true, ci);

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ToString

		public abstract AIntervalEdge<TFinite> Previous();
		public abstract AIntervalEdge<TFinite> Next();

		protected AIntervalEdge(EdgeType nType = default(EdgeType), TFinite oValue = default(TFinite)) {
			Type = nType;
			Value = oValue;
		} // constructor

		protected abstract bool IsValueEqualTo(AIntervalEdge<TFinite> other);
		protected abstract bool IsValueEqualTo(TFinite other);

		protected abstract bool IsValueLessThan(AIntervalEdge<TFinite> other);
		protected abstract bool IsValueLessThan(TFinite other);

		protected virtual string InfinityToString(bool bPositive, CultureInfo oCultureInfo) {
			return (bPositive ? "+" : "-") + "inf";
		} // InfinityToString

		protected virtual string ValueToString() {
			return ValueToString(null, CultureInfo.InvariantCulture);
		} // ValueToString

		protected virtual string ValueToString(string sFormat, CultureInfo ci) {
			return Value.ToString();
		} // ValueToString
	} // class AIntervalEdge
} // namespace Ezbob.ValueIntervals
