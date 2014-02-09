namespace Ezbob.Utils {
	#region class Utils

	public static class MiscUtils {
		#region method MD5

		public static string MD5(string input) {
			return Security.SecurityUtils.MD5(input);
		} // MD5

		#endregion method MD5
	} // class MiscUtils

	#endregion class MiscUtils
} // namespace Ezbob.Utils
