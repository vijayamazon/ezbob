namespace AutomationCalculator {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal class TokenIntersection {
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
				int levenshteinDistance = CalculateLevenshteinDistance(x, y);
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

			/// <summary>
			/// Calculates Levenshtein distance (aka edit distance) between two strings.
			/// The Levenshtein distance between two words is the minimum number of single-character edits
			/// (i.e. insertions, deletions or substitutions) required to change one word into the other.
			/// https://en.wikipedia.org/wiki/Levenshtein_distance
			/// </summary>
			/// <param name="a">First string.</param>
			/// <param name="b">Seconds strings.</param>
			/// <returns>-1 if at least one of the strings is empty, the distance otherwise.</returns>
			private static int CalculateLevenshteinDistance(string a, string b) {
				if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
					return -1;

				int lengthA = a.Length;
				int lengthB = b.Length;
				var distances = new int[lengthA + 1, lengthB + 1];

				for (int i = 0; i <= lengthA; i++)
					distances[i, 0] = i;

				for (int j = 0; j <= lengthB; j++)
					distances[0, j] = j;

				for (int i = 1; i <= lengthA; i++) {
					for (int j = 1; j <= lengthB; j++) {
						int cost = b[j - 1] == a[i - 1] ? 0 : 1;

						distances[i, j] = Math.Min(
							Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
							distances[i - 1, j - 1] + cost
						);
					} // for j
				} // for i

				return distances[lengthA, lengthB];
			} // CalculateLevenshteinDistance
		} // LevenshteinComparer
	} // class TokenIntersection
} // namespace
