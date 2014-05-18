namespace Ezbob.Utils.Security {
	using System;
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;

	public static class SecurityUtils {
		#region public

		#region irreversible

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

		public static string HashPassword(string sUserName, string sPassword) {
			return Hash(sUserName + sPassword);
		} // HashPassword

		public static string HashPassword(string sPassword) {
			return Hash(sPassword);
		} // HashPassword

		#endregion method HashPassword

		#region method Hash

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

		#endregion method Hash

		#endregion irreversible

		#region reversible

		#region method Encrypt

		public static byte[] EncryptBytes(string input) {
			return Encoding.UTF8.GetBytes(Encrypt(input));
		} // EncryptBytes

		public static string Encrypt(string input) {
			return Encrypt<AesManaged>(input, GetKey(), GetSalt());
		} // Encrypt

		public static string Encrypt<T>(string value, string password, string salt) where T : SymmetricAlgorithm, new() {
			DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

			SymmetricAlgorithm algorithm = new T();

			byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
			byte[] rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);

			ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIv);

			using (var buffer = new MemoryStream()) {
				using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write)) {
					using (var writer = new StreamWriter(stream, Encoding.Unicode)) {
						writer.Write(value);
					} // using writer
				} // using stream

				return Convert.ToBase64String(buffer.ToArray());
			} // using buffer
		} // Encrypt

		#endregion method Encrypt

		#region method Decrypt

		public static string Decrypt(byte[] input) {
			return Decrypt(Encoding.UTF8.GetString(input));
		} // Decrypt

		public static string Decrypt(string input) {
			return Decrypt<AesManaged>(input, GetKey(), GetSalt());
		} // Decrypt

		public static string Decrypt<T>(string text, string password, string salt) where T : SymmetricAlgorithm, new() {
			DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

			SymmetricAlgorithm algorithm = new T();

			byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
			byte[] rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);

			ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIv);

			using (var buffer = new MemoryStream(Convert.FromBase64String(text))) {
				using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read)) {
					using (var reader = new StreamReader(stream, Encoding.Unicode)) {
						return reader.ReadToEnd();
					} // using reader
				} // using stream
			} // using buffer
		} // Decrypt

		#endregion method Decrypt

		#endregion reversible

		#endregion public

		#region private

		#region method GetKey

		private static string GetKey() {
			return "F40D3C355CFD5DC4251D9ADEECC7DD73BFB9D5A80946786F071007399130335D";
		} // GetKey

		#endregion method GetKey

		#region method GetSalt

		private static string GetSalt() {
			return "EzBB0b";
		} // GetSalt

		#endregion method GetSalt

		#endregion private
	} // class SecurityUtils
} // namespace Ezbob.Utils.Security
