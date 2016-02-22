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
	} // class SecurityUtils
} // namespace
