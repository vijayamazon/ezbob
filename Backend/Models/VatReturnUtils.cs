namespace Ezbob.Backend.Models {
	using System.Text.RegularExpressions;

	public static class VatReturnUtils {
		public static int BoxNameToNum(string sBoxName) {
			int nBoxNum = 0;

			Match m = ms_oBoxNameRegEx.Match(sBoxName);

			if (m.Success)
				int.TryParse(m.Groups[1].Value, out nBoxNum);

			return nBoxNum;
		} // BoxNameToNum

		private static readonly Regex ms_oBoxNameRegEx = new Regex(@"\(Box (\d+)\)$");
	} // class VatReturnUtils
} // namespace
