namespace Ezbob.Utils.Lingvo {
	public class English : ILanguage {
		public string Number(int nCount, string sSingular, string sPlural = null) {
			if (nCount == 1)
				return nCount + " " + sSingular;

			if (!string.IsNullOrWhiteSpace(sPlural))
				return nCount + " " + sPlural;

			if (string.IsNullOrWhiteSpace(sSingular))
				sSingular = string.Empty;

			string s = sSingular.ToLowerInvariant();

			if (s.EndsWith("ch") || s.EndsWith("x") || s.EndsWith("s"))
				return nCount + " " + sSingular + "es";

			if (s.EndsWith("f"))
				return nCount + " " + sSingular.Substring(0, sSingular.Length - 1) + "ves";

			if (s.EndsWith("fe"))
				return nCount + " " + sSingular.Substring(0, sSingular.Length - 2) + "ves";

			return nCount + " " + sSingular + "s";
		} // Number
	} // class English
} // namespace
