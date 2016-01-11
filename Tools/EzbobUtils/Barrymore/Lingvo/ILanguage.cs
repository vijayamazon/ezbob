namespace Ezbob.Utils.Lingvo {
	public interface ILanguage {
		string Number(int nCount, string sSingular, string sPlural = null);
		string Number(ulong nCount, string sSingular, string sPlural = null);
	} // interface ILanguage
} // namespace Ezbob.Utils.Lingvo
