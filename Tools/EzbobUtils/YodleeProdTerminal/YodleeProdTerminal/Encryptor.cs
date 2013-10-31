namespace YodleeProdTerminal
{
	using System.Security.Cryptography;
	using EKMConnector;

	public static class Encryptor
    {
        public static string Encrypt(string input)
        {
            return CipherUtility.Encrypt<AesManaged>(input, GetKey(), Consts.CryptoSalt);
        }

        public static string Decrypt(string input)
        {
            return CipherUtility.Decrypt<AesManaged>(input, GetKey(), Consts.CryptoSalt);
        }

        private static string GetKey()
        {
            return Consts.CryptoKey;
        }
    }
}