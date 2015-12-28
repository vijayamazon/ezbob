namespace Ezbob.Backend.Models {
	using System;
	using System.Globalization;
	using System.Runtime.Serialization;
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
