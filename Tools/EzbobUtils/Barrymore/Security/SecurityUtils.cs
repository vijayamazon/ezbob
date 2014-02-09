namespace Ezbob.Utils.Security {
	using System;
	using System.Text;

	#region class SecurityUtils

	public class SecurityUtils {
		#region public

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

			return sb.ToString().ToLowerInvariant();
		} // MD5

		#endregion method MD5

		#region method HashPassword

		public static string HashPassword(string sPassword) {
			var sha = System.Security.Cryptography.SHA512.Create();

			byte[] aryInputBytes = System.Text.Encoding.ASCII.GetBytes(sPassword ?? string.Empty);
			byte[] oFirstHash = sha.ComputeHash(aryInputBytes);

			// truncating some characters - salt replacement
			byte[] oSecondHash = sha.ComputeHash(oFirstHash, 6, oFirstHash.Length - 14);

			var sb = new StringBuilder();

			for (int i = 0; i < oSecondHash.Length; i++)
				sb.Append(oSecondHash[i].ToString("X2"));

			return sb.ToString().ToLowerInvariant();
		} // HashPassword

		#endregion method HashPassword

		#endregion public
	} // class SecurityUtils

	#endregion class SecurityUtils
} // namespace Ezbob.Utils.Security
