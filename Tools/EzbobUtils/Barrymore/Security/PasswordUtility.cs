namespace Ezbob.Utils.Security {
	using System;
	using System.Security.Cryptography;
	using System.Text;
	using CryptSharp.Utility;

	public class PasswordUtility {
		public static string HashPasswordOldWay(string userName, string password) {
			return HashPasswordOldWay(userName + password);
		} // HashPasswordOldWay

		/// <summary>
		/// This method can easily be broken so DO NOT use it to hash sensitive data.
		/// It can be used to convert insensitive data into scrambled strings though.
		/// </summary>
		/// <param name="sPassword"></param>
		/// <returns></returns>
		public static string HashPasswordOldWay(string sPassword) {
			var sha = System.Security.Cryptography.SHA512.Create();

			byte[] aryInputBytes = System.Text.Encoding.ASCII.GetBytes(sPassword ?? string.Empty);
			byte[] oFirstHash = sha.ComputeHash(aryInputBytes);

			// truncating some characters - salt replacement
			byte[] oSecondHash = sha.ComputeHash(oFirstHash, 6, oFirstHash.Length - 14);

			var sb = new StringBuilder();

			for (int i = 0; i < oSecondHash.Length; i++)
				sb.Append(oSecondHash[i].ToString("X2"));

			return sb.ToString().ToLowerInvariant();
		} // HashPasswordOldWay

		/// <summary>
		/// Creates hasher instance.
		/// </summary>
		/// <param name="minCycleCount">How many times the password should be hashed.</param>
		public PasswordUtility(int minCycleCount) {
			this.algo = new HashAlgorithm();
			this.minCycleCount = Math.Max(minCycleCount, 30000);
		} // constructor

		/// <summary>
		/// Generates hash for a new user.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <param name="rawPassword">User password (unencrypted).</param>
		/// <returns>Password data ready for serialization (<see cref="HashedPassword"/>).</returns>
		public HashedPassword Generate(string userName, string rawPassword) {
			return Generate(userName, rawPassword, null, null);
		} // Generate

		/// <summary>
		/// Checks whether entered password is equal to the serialized one (e.g. stored in DB).
		/// </summary>
		/// <param name="rawPassword">Password to validate (unencrypted).</param>
		/// <param name="serializedPassword">Serialized password (e.g. password read from DB).</param>
		/// <returns>Validation result, <see cref="PasswordValidationResult"/>.</returns>
		public PasswordValidationResult Validate(string rawPassword, HashedPassword serializedPassword) {
			if (serializedPassword == null)
				return new PasswordValidationResult(false);

			string hashedPassword = serializedPassword.Raw.CycleCount <= 0
				? HashPasswordOldWay(serializedPassword.UserName + rawPassword)
				: HashPassword(rawPassword, serializedPassword);

			var result = new PasswordValidationResult(
				hashedPassword.Equals(serializedPassword.Password, StringComparison.InvariantCulture)
			);

			if (!result)
				return result;

			if (serializedPassword.Raw.CycleCount < this.minCycleCount)
				result.NewPassword = Generate(serializedPassword.UserName, rawPassword, null, null);

			return result;
		} // Validate

		private string HashPassword(string rawPassword, HashedPassword serializedPassword) {
			HashedPassword hashedPassword = Generate(
				serializedPassword.UserName,
				rawPassword,
				serializedPassword.Raw.CycleCount,
				serializedPassword.Raw.Salt
			);

			return hashedPassword.Password;
		} // HashPassword

		private HashedPassword Generate(string userName, string rawPassword, int? cycleCount, byte[] salt) {
			byte[] saltToUse = salt ?? GenerateSalt(userName);

			int iterations = cycleCount ?? this.minCycleCount;

			this.algo.Prepare(userName, rawPassword);

			byte[] hashedPassword = Pbkdf2.ComputeDerivedKey(this.algo.Instance, saltToUse, iterations, this.algo.HashSize);

			return new HashedPassword(userName, iterations, hashedPassword, saltToUse);
		} // Generate

		private static byte[] GenerateSalt(string userName) {
			var prov = new RNGCryptoServiceProvider(userName);
			var salt = new byte[16];
			prov.GetBytes(salt);
			return salt;
		} // GenerateSalt

		private class HashAlgorithm {
			public HashAlgorithm() {
				CreateInstance();
			} // constructor

			public void Prepare(string userName, string rawPassword) {
				if (this.instance.IsDisposed)
					CreateInstance();

				this.instance.Key = System.Text.Encoding.UTF8.GetBytes(userName + (rawPassword ?? string.Empty));

				HashSize = this.instance.HashSize / 8;
			} // Prepare

			public int HashSize { get; private set; }

			public KeyedHashAlgorithm Instance { get { return this.instance; } }

			private void CreateInstance() {
				this.instance = new HmacSha512();
			} // CreateInstance

			private HmacSha512 instance;
		} // class HashAlgorithm

		private class HmacSha512 : System.Security.Cryptography.HMACSHA512 {
			public HmacSha512() {
				IsDisposed = false;
			} // constructor

			public bool IsDisposed { get; private set; }

			/// <summary>
			/// Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.HMAC"/> class
			/// when a key change is legitimate and optionally releases the managed resources.
			/// </summary>
			/// <param name="disposing">true to release both managed and unmanaged resources;
			/// false to release only unmanaged resources. </param>
			protected override void Dispose(bool disposing) {
				IsDisposed = true;
				base.Dispose(disposing);
			} // Dispose
		} // class HmacSha512

		private readonly int minCycleCount;
		private readonly HashAlgorithm algo;
	} // class PasswordUtility
} // namespace
