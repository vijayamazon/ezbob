namespace AutomationCalculator {
	using System.Collections.Generic;
	using System.Linq;

	public class TokenIntersection {
		internal TokenIntersection(string firstString, string secondString) {
			this.first = new HashSet<string>(firstString.Split(' '));
			this.second = new HashSet<string>(secondString.Split(' '));

			var levensteinComparer = new LevenshteinComparer();

			this.intersection = this.first.Intersect(this.second, levensteinComparer).ToList();
		} // constructor

		public int FirstOnlyLength { get { return this.first.Count - IntersectionLength; } }
		public int SecondOnlyLength { get { return this.second.Count - IntersectionLength; } }
		public int IntersectionLength { get { return this.intersection.Count; } }

		private readonly ISet<string> first;
		private readonly ISet<string> second;
		private readonly List<string> intersection;

		private class LevenshteinComparer : IEqualityComparer<string> {
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
			/// <param name="x">The first object of type <paramref name="string"/> to compare.</param>
			/// <param name="y">The second object of type <paramref name="string"/> to compare.</param>
			public bool Equals(string x, string y) {
				int levenshteinDistance = Utils.CalculateLevenshteinDistance(x, y);
				return (0 <= levenshteinDistance) && (levenshteinDistance <= 2);
			} // Equals

			/// <summary>
			/// Returns a hash code for the specified object.
			/// </summary>
			/// <returns>
			/// A hash code for the specified object.
			/// </returns>
			/// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
			/// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type
			/// and <paramref name="obj"/> is null.</exception>
			public int GetHashCode(string obj) {
				if (obj == null)
					return 0;

				return obj.GetHashCode();
			} // GetHashCode
		} // LevenshteinComparer
	} // class TokenIntersection
} // namespace
