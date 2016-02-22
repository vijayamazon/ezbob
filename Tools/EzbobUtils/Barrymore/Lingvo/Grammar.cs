namespace Ezbob.Utils.Lingvo {
	public class Grammar {
		static Grammar() {
			ms_oLingvo = new English();
		} // static constructor

		public static string Number(int nCount, string sSingular, string sPlural = null) {
			return ms_oLingvo.Number(nCount, sSingular, sPlural);
		} // Number

		public static string Number(ulong nCount, string sSingular, string sPlural = null) {
			return ms_oLingvo.Number(nCount, sSingular, sPlural);
		} // Number

		private static readonly ILanguage ms_oLingvo;
	} // class Grammar
} // namespace
