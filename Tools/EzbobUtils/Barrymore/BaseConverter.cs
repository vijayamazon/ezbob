namespace Ezbob.Utils {
	using System;

	public static class BaseConverter {
		public static string Execute(int nValue, char[] aryBaseDigits) {
			if (aryBaseDigits == null)
				throw new ArgumentNullException("aryBaseDigits", "Not base digits specified.");

			if (aryBaseDigits.Length < 2)
				throw new Exception("At least two base digits should be specified.");

			// 32 is the worst cast buffer size for base 2 and int.MaxValue
			const int nMaxLen = 32;

			int i = nMaxLen;
			var buffer = new char[i];
			int targetBase = aryBaseDigits.Length;

			do {
				buffer[--i] = aryBaseDigits[nValue % targetBase];
				nValue = nValue / targetBase;
			} while (nValue > 0);

			var result = new char[nMaxLen - i];
			Array.Copy(buffer, i, result, 0, nMaxLen - i);

			return new string(result);
		} // Execute

		public static readonly char[] LowerCaseLetters = new [] {
			'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
		};

		public static readonly char[] UpperCaseLetters = new [] {
			'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
		};

		public static readonly char[] Letters = new [] {
			'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
			'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
		};
	} // class BaseConverter
} // namespace Ezbob.Utils
