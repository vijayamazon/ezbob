namespace Ezbob.Matrices.Core {
	using System;
	using Ezbob.ValueIntervals;

	public class DecimalIntervalMatrixInterval : TInterval<decimal> {
		public DecimalIntervalMatrixInterval(
			DecimalIntervalMatrixIntervalEdge left,
			DecimalIntervalMatrixIntervalEdge right
		) : base(left, right) {} // constructor

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has the following meanings: Value Meaning Less than zero This object is less than
		/// the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>.
		/// Greater than zero This object is greater than <paramref name="other"/>. 
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public override int CompareTo(TInterval<decimal> other) {
			if (other == null)
				throw new ArgumentNullException();

			return Right.CompareTo(other.Right);
		} // CompareTo

		public virtual int CompareTo(DecimalIntervalMatrixInterval other) {
			return CompareTo((TInterval<decimal>)other);
		} // CompareTo
	} // class DecimalIntervalMatrixInterval
} // namespace
