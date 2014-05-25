namespace Ezbob.Utils.Security {
	using System;
	using System.Text;

	public class SimplePassword {
		#region operator to string

		public static implicit operator string(SimplePassword pwd) {
			return pwd.ToString();
		} // operator to string

		#endregion operator to string

		#region constructor

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

		#endregion constructor

		#region property Password

		public string Password { get; private set; }

		#endregion property Password

		#region property RawValue

		public string RawValue {
			get { return Password; }
		} // RawValue

		#endregion property RawValue

		#region property UserName

		public string UserName { get; private set; }

		#endregion property UserName

		#region property Encrypted

		public Encrypted Encrypted {
			get { return new Encrypted(Password); }
		} // Encrypted

		#endregion property Encrypted

		#region property Hash

		public string Hash {
			get { return SecurityUtils.HashPassword(UserName, Password); }
		} // Hash

		#endregion property Hash

		#region method ToString

		public override string ToString() {
			return Hash;
		} // ToString

		#endregion method ToString
	} // class SimplePassword
} // namespace Ezbob.Utils.Security
