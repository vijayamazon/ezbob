namespace AutomationCalculator {
	using System.Collections.Generic;
	using System.Linq;

	public class AlienTokens {
		internal AlienTokens(string firstString, string secondString) {
			this.inFirst = new List<string>();
			this.inSecond = new List<string>();

			this.first = new HashSet<string>(firstString.Split(' '));
			this.second = new HashSet<string>(secondString.Split(' '));

			var levensteinComparer = new LevenshteinComparer();

			this.inFirst.AddRange(this.first.Except(this.second, levensteinComparer));
			this.inSecond.AddRange(this.second.Except(this.first, levensteinComparer));
		} // constructor

		public IReadOnlyCollection<string> First { get { return this.first.ToList().AsReadOnly(); } }
		public IReadOnlyCollection<string> Second { get { return this.second.ToList().AsReadOnly(); } }

		/// <summary>
		/// Gets tokens that appear in the first string but not in the second.
		/// </summary>
		public IReadOnlyCollection<string> InFirstOnly { get { return this.inFirst.AsReadOnly(); } }

		/// <summary>
		/// Gets tokens that appear in the second string but not in the first.
		/// </summary>
		public IReadOnlyCollection<string> InSecondOnly { get { return this.inSecond.AsReadOnly(); } }

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
				return (0 <= levenshteinDistance) && (levenshteinDistance <= 1);
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

		private readonly ISet<string> first;
		private readonly ISet<string> second;
		private readonly List<string> inFirst;
		private readonly List<string> inSecond;
	} // class AlienTokens
} // namespace
