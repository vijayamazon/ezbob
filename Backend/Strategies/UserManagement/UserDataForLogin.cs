namespace EzBob.Backend.Strategies.UserManagement {
	using System;
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

		#region class Result

		public class Result : AResultRow {
			#region DB properties

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

			#endregion DB properties

			#region property FailCount

			public int FailCount {
				get { return LoginFailedCount.HasValue ? LoginFailedCount.Value : 0; }
			} // FailCount

			#endregion property FailCount

			#region property IsOldPasswordStyle

			public bool IsOldPasswordStyle {
				get { return string.IsNullOrWhiteSpace(EzPassword); } // get
			} // IsOldPasswordStyle

			#endregion property IsOldPasswordStyle

			#region method IsPasswordValid

			public bool IsPasswordValid(string sPassword) {
				if (IsOldPasswordStyle) {
					return
						(Password == PasswordEncryptor.EncodePassword(sPassword, Email, CreationDate)) ||
						(Password == PasswordEncryptor.EncodeOldPassword(sPassword));
				} // if

				return EzPassword == Ezbob.Utils.Security.SecurityUtils.HashPassword(Email, sPassword);
			} // IsPasswordValid

			#endregion method IsPasswordValid

			#region method ToString

			public override string ToString() {
				var os = new StringBuilder();

				this.Traverse((oInstance, oPropertyInfo) => {
					var oValue = oPropertyInfo.GetValue(oInstance);

					if (oValue == null)
						os.AppendFormat("\t{0}: -- NULL --.\n", oPropertyInfo.Name);
					else
						os.AppendFormat("\t{0}: '{1}'.\n", oPropertyInfo.Name, oValue.ToString());
				});

				return "(\n" + os.ToString() + ")";
			} // ToString

			#endregion method ToString
		} // class Result

		#endregion class Result
	} // class UserDataForLogin
} // namespace EzBob.Backend.Strategies.UserManagement
