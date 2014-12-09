namespace Ezbob.Utils.Security {
	using System.Text;

	public static class SecurityUtils {

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

		public static string HashPassword(string sUserName, string sPassword) {
			return Hash(sUserName + sPassword);
		} // HashPassword

		public static string HashPassword(string sPassword) {
			return Hash(sPassword);
		} // HashPassword

		public static string Hash(string sPassword) {
			var sha = System.Security.Cryptography.SHA512.Create();

			byte[] aryInputBytes = System.Text.Encoding.ASCII.GetBytes(sPassword ?? string.Empty);
			byte[] oFirstHash = sha.ComputeHash(aryInputBytes);

			// truncating some characters - salt replacement
			byte[] oSecondHash = sha.ComputeHash(oFirstHash, 6, oFirstHash.Length - 14);

			var sb = new StringBuilder();

			for (int i = 0; i < oSecondHash.Length; i++)
				sb.Append(oSecondHash[i].ToString("X2"));

			return sb.ToString().ToLowerInvariant();
		} // Hash

	} // class SecurityUtils
} // namespace
