namespace Ezbob.Utils.Security {
	using System;
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;

	public class Encrypted {

		public static string Decrypt(Encrypted input) {
			return input.Decrypt();
		} // Decrypt

		public static string Decrypt(byte[] input) {
			return Decrypt(Encoding.UTF8.GetString(input));
		} // Decrypt

		public static string Decrypt(string input) {
			return InternalDecrypt(input);
		} // Decrypt

		public static implicit operator byte[](Encrypted obj) {
			return Encoding.UTF8.GetBytes(obj.m_sEncrypted);
		} // to string

		public static implicit operator string(Encrypted obj) {
			return obj.m_sEncrypted;
		} // to string

		public Encrypted(string sUnencrypted) {
			m_sEncrypted = InternalEncrypt(sUnencrypted);
		} // constructor

		public string Decrypt() {
			return Decrypt(m_sEncrypted);
		} // Decrypt

		public override string ToString() {
			return m_sEncrypted;
		} // ToString

		private readonly string m_sEncrypted;

		private static string InternalEncrypt(string value) {
			string password = GetKey();
			string salt = GetSalt();

			DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

			SymmetricAlgorithm algorithm = new AesManaged();

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
		} // InternalEncrypt

		private static string InternalDecrypt(string text) {
			string password = GetKey();
			string salt = GetSalt();

			DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

			SymmetricAlgorithm algorithm = new AesManaged();

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
		} // InternalDecrypt

		private static string GetKey() {
			return "F40D3C355CFD5DC4251D9ADEECC7DD73BFB9D5A80946786F071007399130335D";
		} // GetKey

		private static string GetSalt() {
			return "EzBB0b";
		} // GetSalt

	} // class Encrypted
} // namespace
