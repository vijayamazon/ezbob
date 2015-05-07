namespace Ezbob.Backend.Strategies.Extensions {
	public static class StringExt {
		public static bool IsEqualTo(this string a, string b) {
			return string.IsNullOrEmpty(a) ? string.IsNullOrEmpty(b) : string.Equals(a, b);
		} // IsEqualTo
	} // class StringExt
} // namespace
