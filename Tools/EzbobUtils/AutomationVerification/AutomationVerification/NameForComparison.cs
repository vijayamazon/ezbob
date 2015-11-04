namespace AutomationCalculator {
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;

	public class NameForComparison {
		public NameForComparison(string name) {
			Init(name);
		} // constructor

		public NameForComparison(string name, string altName) {
			Init(name);

			if (AdjustedName == string.Empty)
				Init(altName);
		} // constructor

		/// <summary>
		/// Name as it was passed to constructor.
		/// </summary>
		public string RawName { get; private set; }

		/// <summary>
		/// Name after initial adjustment.
		/// </summary>
		public string AdjustedName {
			get {
				if (this.adjustedName != null)
					return this.adjustedName;

				if (string.IsNullOrWhiteSpace(RawName))
					this.adjustedName = string.Empty;
				else {

					string[] words = whiteSpaces.Split(RawName
						.Trim()
						.ToLowerInvariant()
						.Replace("&amp;", "&")
						.Replace(".", string.Empty)
						.Replace("#049;", "'")
						.Replace('(', ' ')
						.Replace(':', ' ')
						.Replace(')', ' ')
						.Trim()
					);

					this.adjustedName = string.Join(
						" ",
						words.Where(w => w != "the").Select(w => w == "limited" ? "ltd" : w)
					);
				} // if

				return this.adjustedName;
			} // get
		} // AdjustedName

		/// <summary>
		/// Adjusted name with all the '-' replaced with spaces.
		/// </summary>
		public string PlainName {
			get {
				if (this.plainName == null)
					this.plainName = AdjustedName.Replace('-', ' ');

				return this.plainName;
			} // get
		} // PlainName

		/// <summary>
		/// Adjusted name with stopwords dropped.
		/// </summary>
		public string NoStopwordsName {
			get {
				if (this.noStopwordsName == null) {
					this.noStopwordsName = string.Join(
						" ",
						whiteSpaces.Split(AdjustedName).Where(w => !stopwords.Contains(w))
					);
				} // if

				return this.noStopwordsName;
			} // get
		} // NoStopwordsName

		public bool SameAsPerson(string otherName) {
			if ((AdjustedName == string.Empty) || string.IsNullOrWhiteSpace(otherName))
				return false;

			return SameAsCompany(new NameForComparison(otherName));
		} // SameAsPerson

		public bool SameAsCompany(NameForComparison other) {
			if (other == null)
				return false;

			if ((AdjustedName == string.Empty) || (other.AdjustedName == string.Empty))
				return false;

			if (AdjustedName == other.AdjustedName)
				return true;

			if (PlainName.HasSubstring(other.PlainName))
				return true;

			if (other.PlainName.HasSubstring(PlainName))
				return true;

			if ((NoStopwordsName == string.Empty) || (other.NoStopwordsName == string.Empty))
				return false;

			if (NoStopwordsName == other.NoStopwordsName)
				return true;

			if (NoStopwordsName.Replace(" ", "") == other.NoStopwordsName.Replace(" ", ""))
				return true;

			var ti = new TokenIntersection(NoStopwordsName, other.NoStopwordsName);

			switch (ti.IntersectionLength) {
			case 0:
				return false;

			case 1:
				return
					(ti.FirstOnlyLength <= 1) &&
					(ti.SecondOnlyLength <= 1) &&
					(ti.FirstOnlyLength + ti.SecondOnlyLength <= 1);

			default:
				return (ti.FirstOnlyLength <= 1) && (ti.SecondOnlyLength <= 1);
			} // switch
		} // SameAsCompany

		private void Init(string name) {
			RawName = name;
			this.adjustedName = null;
			this.plainName = null;
			this.noStopwordsName = null;
		} // Init

		private string adjustedName;
		private string plainName;
		private string noStopwordsName;

		private static readonly Regex whiteSpaces = new Regex(@"\s+");
		private static readonly Stopwords stopwords = new Stopwords();
	} // class NameForComparison

	internal static class NameForComparisonExt {
		public static bool HasSubstring(this string haystack, string needle) {
			if (string.IsNullOrWhiteSpace(haystack) || string.IsNullOrWhiteSpace(needle))
				return false;

			return haystack.IndexOf(needle, StringComparison.InvariantCulture) >= 0;
		} // HasSubstring
	} // class NameForComparisonExt
} // namespace
