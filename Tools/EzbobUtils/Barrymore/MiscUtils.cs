namespace Ezbob.Utils {
	using System;
	using System.Text;
	using System.Xml;

	#region class Utils

	public static class MiscUtils {
		#region method MD5

		public static string MD5(string input) {
			// step 1, calculate MD5 hash from input
			var md5 = System.Security.Cryptography.MD5.Create();

			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input ?? string.Empty);
			byte[] hash = md5.ComputeHash(inputBytes);

			// step 2, convert byte array to hex string
			var sb = new StringBuilder();

			for (int i = 0; i < hash.Length; i++)
				sb.Append(hash[i].ToString("X2"));

			return sb.ToString();
		} // MD5

		#endregion method MD5
	} // class MiscUtils

	#endregion class MiscUtils
} // namespace Ezbob.Utils
