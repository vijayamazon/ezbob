namespace Ezbob.Utils.Lingvo {
	using System.Globalization;

	public class English : ILanguage {
		public string Number(int nCount, string sSingular, string sPlural = null) {
			if (nCount == 1)
				return "1 " + sSingular;

			string sCount = nCount == 0 ? "no" : nCount.ToString("N0", CultureInfo.InvariantCulture);

			return Number(sCount, sSingular, sPlural);
		} // Number

		public string Number(ulong nCount, string sSingular, string sPlural = null) {
			if (nCount == 1)
				return "1 " + sSingular;

			string sCount = nCount == 0 ? "no" : nCount.ToString("N0", CultureInfo.InvariantCulture);

			return Number(sCount, sSingular, sPlural);
		} // Number

		private string Number(string sCount, string sSingular, string sPlural) {
			if (!string.IsNullOrWhiteSpace(sPlural))
				return sCount + " " + sPlural;

			if (string.IsNullOrWhiteSpace(sSingular))
				sSingular = string.Empty;

			string s = sSingular.ToLowerInvariant();

			if (s.EndsWith("ch") || s.EndsWith("x") || s.EndsWith("s"))
				return sCount + " " + sSingular + "es";

			if (s.EndsWith("f"))
				return sCount + " " + sSingular.Substring(0, sSingular.Length - 1) + "ves";

			if (s.EndsWith("fe"))
				return sCount + " " + sSingular.Substring(0, sSingular.Length - 2) + "ves";

			return sCount + " " + sSingular + "s";
		} // Number
	} // class English
} // namespace
