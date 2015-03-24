namespace Ezbob.API.AuthenticationAPI
{
	using System;
	using System.Security.Cryptography;
	using System.Text;
	using Newtonsoft.Json;

	public class Helper
    {

		public static string CACHE_KEY_SEPARATOR = "-";

		public static string GetHash(string input) {
			HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

			byte[] byteValue = Encoding.UTF8.GetBytes(input);

			byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

			return Convert.ToBase64String(byteHash);
		}

		public static JsonSerializerSettings JsonReferenceLoopHandling (){

			return new JsonSerializerSettings { Formatting = Formatting.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
		}
    }
}