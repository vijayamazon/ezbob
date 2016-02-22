namespace Ezbob.Backend.Models {
	using System;
	using System.Globalization;
	using System.Runtime.Serialization;
	using System.Text;
	using Utils.Security;

	[DataContract]
	public class DasKennwort {
		public DasKennwort(string rawPassword = null) {
			Data = rawPassword;
		} // constructor

		[DataMember]
		public string Data {
			get {
				if (string.IsNullOrWhiteSpace(this.data))
					return string.Empty;

				try {
					return Encrypted.Decrypt(this.data);
				} catch {
					return string.Empty;
				} // try
			} // get

			set {
				this.data = string.IsNullOrWhiteSpace(value) ? string.Empty : new Encrypted(value).ToString();
				this.setTime = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
			} // set
		} // Data

		public void GenerateSimplePassword(int nLength) {
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

			Data = osPassword.ToString();
		} // GenerateSimplePassword

		public override string ToString() {
			return this.setTime;
		} // ToString

		private string setTime;
		private string data;
	} // class DasKennwort

	public static class DasKennwortExt {
		public static string Decrypt(this DasKennwort s) {
			return s == null ? string.Empty : s.Data;
		} // Decrypt
	} // class DasKennwortExt
} // namespace
