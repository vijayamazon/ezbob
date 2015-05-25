namespace ListNotAutoApprovedReasons {
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;

	class NonAffirmativeGroupKey : IComparable<NonAffirmativeGroupKey>, IEqualityComparer<NonAffirmativeGroupKey> {
		public NonAffirmativeGroupKey(Trail trail) {
			Length = 0;
			Hash = string.Empty;
			List = string.Empty;

			List<string> nonAffirmative = trail.Reasons;

			if (nonAffirmative == null)
				return;

			if (nonAffirmative.Count <= 0)
				return;

			Length = nonAffirmative.Count;
			List = string.Join(", ", nonAffirmative);

			string[] ary = nonAffirmative.ToArray();
			Array.Sort(ary);
			Hash = MiscUtils.MD5(string.Join(",", ary));
		} // constructor

		public NonAffirmativeGroupKey(int length) {
			Length = length;
			Hash = "Total";
			List = string.Empty;
		} // constructor

		public int Length { get; private set; }
		public string Hash { get; private set; }
		public string List { get; private set; }

		/// <summary>Compares the current object with another object of the same type.</summary>
		/// <returns>A value that indicates the relative order of the objects being compared.
		/// The return value has the following meanings: Value Meaning Less than zero This
		/// object is less than the <paramref name="other"/> parameter.Zero This object is
		/// equal to <paramref name="other"/>.
		/// Greater than zero This object is greater than <paramref name="other"/>. 
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public int CompareTo(NonAffirmativeGroupKey other) {
			if (other == null)
				throw new NullReferenceException("Cannot compare NonAffirmativeGroupKey to null.");

			int lengthOrder = Length.CompareTo(other.Length);

			return lengthOrder != 0
				? lengthOrder
				: String.Compare(Hash, other.Hash, StringComparison.InvariantCultureIgnoreCase);
		} // CompareTo

		public bool Equals(NonAffirmativeGroupKey x, NonAffirmativeGroupKey y) {
			if (ReferenceEquals(x, y))
				return true;

			if ((x == null) || (y == null))
				return false;

			return x.Hash == y.Hash;
		} // Equals

		public int GetHashCode(NonAffirmativeGroupKey obj) {
			if (obj == null)
				throw new ArgumentNullException("obj", "Cannot get hash code of null (NonAffirmativeGroupKey).");

			return obj.Hash.GetHashCode();
		} // GetHashCode
	} // NonAffirmativeGroupKey
} // namespace
