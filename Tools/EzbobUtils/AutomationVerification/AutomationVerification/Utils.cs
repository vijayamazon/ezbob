namespace AutomationCalculator {
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;

	public static class Utils {
		public static string AdjustCompanyName(string companyName) {
			if (string.IsNullOrWhiteSpace(companyName))
				return string.Empty;

			string[] words = whiteSpaces.Split(companyName
				.Trim()
				.ToLowerInvariant()
				.Replace("&amp;", "&")
				.Replace(".", string.Empty)
				.Replace("#049;", "'")
			);

			return string.Join(" ", words.Where(w => w != "the").Select(w => w == "limited" ? "ltd" : w));
		} // AdjustCompanyName

		public static string DropStopWords(string companyName) {
			if (string.IsNullOrWhiteSpace(companyName))
				return string.Empty;

			return string.Join(
				" ",
				whiteSpaces.Split(companyName).Where(w => !stopwords.Contains(w))
			);
		} // DropStopWords

		public static AlienTokens FindAlienTokens(string firstName, string secondName) {
			return new AlienTokens(firstName, secondName);
		} // FindAlienTokens

		/// <summary>
		/// Calculates Levenshtein distance (aka edit distance) between two strings.
		/// The Levenshtein distance between two words is the minimum number of single-character edits
		/// (i.e. insertions, deletions or substitutions) required to change one word into the other.
		/// https://en.wikipedia.org/wiki/Levenshtein_distance
		/// </summary>
		/// <param name="a">First string.</param>
		/// <param name="b">Seconds strings.</param>
		/// <returns>-1 if at least one of the strings is empty, the distance otherwise.</returns>
		public static int CalculateLevenshteinDistance(string a, string b) {
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

		private static readonly Regex whiteSpaces = new Regex(@"\s+");
		private static readonly Stopwords stopwords = new Stopwords();
	} // class Utils
} // namespace
