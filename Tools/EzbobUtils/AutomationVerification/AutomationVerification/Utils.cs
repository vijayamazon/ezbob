namespace AutomationCalculator {
	public static class Utils {
		public static string AdjustCompanyName(string companyName) {
			if (string.IsNullOrWhiteSpace(companyName))
				return string.Empty;

			return companyName.Trim()
				.ToLowerInvariant()
				.Replace("limited", "ltd")
				.Replace("the ", string.Empty)
				.Replace("&amp;", "&")
				.Replace(".", string.Empty)
				.Replace("#049;", "'");
		} // AdjustCompanyName
	} // class Utils
} // namespace
