namespace Ezbob.Utils 
{
	using System;
	using System.Security.Cryptography;
	using System.Text;

	public static class PasswordEncryptor
	{
		private static readonly byte[] hmacsha1Key = new byte[]
				{
					217, 197, 36, 73, 245, 170, 52, 86, 16, 196, 190, 197, 158, 222, 60, 108, 212, 45, 234, 232, 27, 169, 165, 13, 12,
					242, 30, 203, 10, 229, 81, 42, 201, 35, 31, 194, 112, 159, 161, 77, 44, 125, 4, 25, 109, 92, 211, 39, 80, 117, 230,
					173, 106, 87, 105, 195, 62, 171, 89, 189, 230, 39, 60, 148
				};

		public static string EncodePassword(string password, string userName, DateTime creationDate)
		{
			var hMacsha = new HMACSHA1 { Key = hmacsha1Key };
			string combined = userName.ToUpperInvariant() + password + creationDate.ToString("dd-MM-yyyy hh:mm:ss");
			return Convert.ToBase64String(hMacsha.ComputeHash(Encoding.Unicode.GetBytes(combined)));
		}

		public static string EncodeOldPassword(string password)
		{
			var hMacsha = new HMACSHA1 { Key = hmacsha1Key };
			return Convert.ToBase64String(hMacsha.ComputeHash(Encoding.Unicode.GetBytes(password)));
		}
	}
}
