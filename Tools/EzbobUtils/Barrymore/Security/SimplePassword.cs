namespace Ezbob.Utils.Security {
	using System;
	using System.Text;

	public class SimplePassword {

		public static implicit operator string(SimplePassword pwd) {
			return pwd.ToString();
		} // operator to string

		public SimplePassword(int nLength, string sUserName = null) {
			var osPassword = new StringBuilder();

			const int cLowerMin = 'a';
			const int cLowerMax = 1 + (int)'z';

			const int cUpperMin = 'A';
			const int cUpperMax = 1 + (int)'Z';

			const int cDigitMin = '0';
			const int cDigitMax = 1 + (int)'9';

			var rnd = new Random();

			for (int i = 1; i <= Math.Max(nLength, 4); i++) {
				if (i % 3 == 0)
					osPassword.Append((char)rnd.Next(cDigitMin, cDigitMax));
				else if (i % 2 == 0)
					osPassword.Append((char)rnd.Next(cUpperMin, cUpperMax));
				else
					osPassword.Append((char)rnd.Next(cLowerMin, cLowerMax));
			} // for

			Password = osPassword.ToString();
			UserName = sUserName ?? string.Empty;
		} // constructor

		public string Password { get; private set; }

		public string RawValue {
			get { return Password; }
		} // RawValue

		public string UserName { get; private set; }

		public Encrypted Encrypted {
			get { return new Encrypted(Password); }
		} // Encrypted

		public string Hash {
			get { return SecurityUtils.HashPassword(UserName, Password); }
		} // Hash

		public override string ToString() {
			return Hash;
		} // ToString

	} // class SimplePassword
} // namespace
