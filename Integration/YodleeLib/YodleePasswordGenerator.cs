namespace YodleeLib
{
	using System;
	using System.Text;

	public class YodleePasswordGenerator
	{
		public static string GenerateRandomPassword()
		{
			var rnd = new Random();
			var sb = new StringBuilder();
			sb.Append(GenerateLowercaseLetter(rnd));
			sb.Append(GenerateLowercaseLetter(rnd));
			sb.Append(GenerateUppercaseLetter(rnd));
			sb.Append(GenerateLowercaseLetter(rnd));
			sb.Append(GenerateUppercaseLetter(rnd));
			sb.Append(GenerateDigit(rnd));
			sb.Append(GenerateDigit(rnd));
			sb.Append(GenerateDigit(rnd));
			return sb.ToString();
		}

		private static int GenerateDigit(Random rnd)
		{
			return rnd.Next(10);
		}

		private static char GenerateLowercaseLetter(Random rnd)
		{
			return (char)(rnd.Next(26) + 65);
		}

		private static char GenerateUppercaseLetter(Random rnd)
		{
			return (char)(rnd.Next(26) + 97);
		}
	}
}