namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Security.Cryptography;
	using System.Text;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using JetBrains.Annotations;

	internal class UserDataForLogin : AStoredProcedure {
		public UserDataForLogin(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

		public override bool HasValidParameters() {
			return !string.IsNullOrWhiteSpace(Email);
		} // HasValidParameters

		public string Email { get; set; }

		public class Result : AResultRow {

			public int UserID { get; set; }
			public string Email { get; set; }
			public string Password { get; set; }
			public string EzPassword { get; set; }
			public DateTime CreationDate { get; set; }
			public int IsDeleted { get; set; }
			public DateTime? DisableDate { get; set; }
			public bool? ForcePassChange { get; set; }
			public DateTime? PassSetTime { get; set; }
			public int? PassExpPeriod { get; set; }
			public bool? DisablePassChange { get; set; }

			[UsedImplicitly]
			public int? LoginFailedCount { get; set; }

			public int FailCount {
				get { return LoginFailedCount.HasValue ? LoginFailedCount.Value : 0; }
			} // FailCount

			public bool IsOldPasswordStyle {
				get { return string.IsNullOrWhiteSpace(EzPassword); } // get
			} // IsOldPasswordStyle

			public bool IsPasswordValid(string sPassword) {
				if (IsOldPasswordStyle) {
					return
						(Password == HashPassword(sPassword, Email, CreationDate)) ||
						(Password == HashPassword(sPassword));
				} // if

				return EzPassword == Ezbob.Utils.Security.SecurityUtils.HashPassword(Email, sPassword);
			} // IsPasswordValid

			public override string ToString() {
				var os = new StringBuilder();

				this.Traverse((oInstance, oPropertyInfo) => {
					var oValue = oPropertyInfo.GetValue(oInstance);

					if (oValue == null)
						os.AppendFormat("\t{0}: -- NULL --.\n", oPropertyInfo.Name);
					else
						os.AppendFormat("\t{0}: '{1}'.\n", oPropertyInfo.Name, oValue.ToString());
				});

				return "(\n" + os + ")";
			} // ToString

			// This is an old password encryption method that we got from S c o r t o.
			// It can be removed once we are not using Password field in Security_User.

			private static string HashPassword(string password, string userName, DateTime creationDate) {
				var hMacsha = new HMACSHA1 { Key = ms_oKey };
				string combined = userName.ToUpperInvariant() + password + creationDate.ToString("dd-MM-yyyy HH:mm:ss");
				return Convert.ToBase64String(hMacsha.ComputeHash(Encoding.Unicode.GetBytes(combined)));
			} // HashPassword

			private static string HashPassword(string password) {
				var hMacsha = new HMACSHA1 { Key = ms_oKey };
				return Convert.ToBase64String(hMacsha.ComputeHash(Encoding.Unicode.GetBytes(password)));
			} // HashPassword

			private static readonly byte[] ms_oKey = new byte[] {
				217, 197, 36, 73, 245, 170, 52, 86, 16, 196, 190, 197, 158, 222, 60, 108, 212, 45, 234, 232, 27, 169, 165, 13, 12,
				242, 30, 203, 10, 229, 81, 42, 201, 35, 31, 194, 112, 159, 161, 77, 44, 125, 4, 25, 109, 92, 211, 39, 80, 117, 230,
				173, 106, 87, 105, 195, 62, 171, 89, 189, 230, 39, 60, 148
			};

		} // class Result

	} // class UserDataForLogin
} // namespace Ezbob.Backend.Strategies.UserManagement
