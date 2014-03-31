namespace Ezbob.Utils {
	using System;

	#region class Utils

	public static class MiscUtils {
		#region method MD5

		public static string MD5(string input) {
			return Security.SecurityUtils.MD5(input);
		} // MD5

		#endregion method MD5

		#region method Validate

		public static string ValidateStringArg(string sValue, string sArgName, bool bThrow = true, int nMaxAllowedLength = 255) {
			sValue = (sValue ?? string.Empty).Trim();

			if (sValue.Length == 0) {
				if (bThrow)
					throw new ArgumentNullException(sArgName, sArgName + " not specified.");

				return sValue;
			} // if

			if (sValue.Length > nMaxAllowedLength) {
				if (bThrow)
					throw new Exception(sArgName + " is too long.");

				return sValue.Substring(0, nMaxAllowedLength);
			} // if

			return sValue;
		} // Validate

		#endregion method Validate
	} // class MiscUtils

	#endregion class MiscUtils
} // namespace Ezbob.Utils
