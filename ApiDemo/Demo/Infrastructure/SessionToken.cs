namespace Demo.Infrastructure {
	using System;
	using System.Globalization;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;

	internal class SessionToken {
		public static SessionToken Deserialize(string sToken) {
			try {
				string sDecoded = Encrypted.Decrypt(Convert.FromBase64String(sToken));

				string[] ary = sDecoded.Split(new string[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

				if (ary.Length != 4) {
					ms_oLog.Alert(
						"Supplied token '{0}' has been de-serialised successfully to '{1}' but contains unexpected number of fields.",
						sToken,
						sDecoded
					);

					return null;
				} // if

				return new SessionToken(ary[0], ary[1], ary[2], ary[3]);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to de-serialise session token from '{0}'.", sToken);
				return null;
			} // try
			
		} // Deserialize

		public SessionToken(string sUserName, string sRemoteAddress) {
			UserName = sUserName ?? string.Empty;
			RemoteAddress = sRemoteAddress ?? string.Empty;
			Random = Guid.NewGuid();
			IssueTime = DateTime.UtcNow;

			Init();
		} // constructor

		public string UserName { get; private set; }

		public string RemoteAddress { get; private set; }

		public Guid Random { get; private set; }

		public DateTime IssueTime { get; private set; }

		public string Packed { get; private set; }

		public string Encoded { get; private set; }

		public override string ToString() {
			return Encoded;
		} // ToString

		private SessionToken(string sRandom, string sUserName, string sRemoteAddress, string sIssuedTime) {
			// No need to try {} catch {} here because this one is called from with a try-block only.

			Random = new Guid(sRandom);
			UserName = sUserName;
			RemoteAddress = sRemoteAddress;
			IssueTime = DateTime.ParseExact(
				sIssuedTime,
				TimeFormat,
				Culture,
				DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
			);

			Init();
		} // constructor

		private void Init() {
			Packed = string.Format(
				"{1}{0}{2}{0}{3}{0}{4}", Separator,
				Random.ToString("N"), UserName, RemoteAddress, IssueTime.ToString(TimeFormat, Culture)
			);

			Encoded = Convert.ToBase64String(new Encrypted(Packed));
		} // Init

		private const string TimeFormat = "yyyy-MM-dd HH:mm:ss.ffffff";
		private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
		private static readonly string Separator = string.Empty + (char)1;
		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(SecurityStub));
	} // class SessionToken
} // namespace
