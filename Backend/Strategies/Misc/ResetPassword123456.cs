namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	[DataContract]
	public enum PasswordResetTarget {
		[EnumMember]
		Customer,

		[EnumMember]
		Broker,
	} // enum PasswordResetTarget

	public class ResetPassword123456 : AStrategy {
		#region public

		#region constructor

		public ResetPassword123456(
			int nTargetID,
			PasswordResetTarget nTarget,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_oSpLoad = new LoadEmailForPasswordReset(DB, Log) {
				TargetID = nTargetID,
				Target = nTarget.ToString(),
			};
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "ResetPassword123456"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Log.Debug("Resetting password for {0} {1}...", m_oSpLoad.Target, m_oSpLoad.TargetID);

			string sEmail = m_oSpLoad.ExecuteScalar<string>();

			if (string.IsNullOrWhiteSpace(sEmail)) {
				Log.Warn("Resetting password for {0} {1} failed: no email found.", m_oSpLoad.Target, m_oSpLoad.TargetID);
				return;
			} // if

			var sp = new SavePassword(m_oSpLoad, DB, Log) {
				Password = SecurityUtils.HashPassword(sEmail, "123456"),
			};

			sp.ExecuteNonQuery();

			Log.Debug("Resetting password for {0} {1} complete.", m_oSpLoad.Target, m_oSpLoad.TargetID);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly LoadEmailForPasswordReset m_oSpLoad;

		#region class LoadEmailForPasswordReset

		private class LoadEmailForPasswordReset : AStoredProcedure {
			public LoadEmailForPasswordReset(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				if (TargetID <= 0)
					return false;

				PasswordResetTarget t;

				return Enum.TryParse<PasswordResetTarget>(Target, out t);
			} // HasValidParameters

			[UsedImplicitly]
			public int TargetID { get; set; }

			[UsedImplicitly]
			public string Target { get; set; }
		} // class LoadEmailForPasswordReset

		#endregion class LoadEmailForPasswordReset

		#region class SavePassword

		private class SavePassword : AStoredProcedure {
			public SavePassword(LoadEmailForPasswordReset oSpLoad, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				TargetID = oSpLoad.TargetID;
				Target = oSpLoad.Target;
			} // constructor

			public override bool HasValidParameters() {
				if ((TargetID <= 0) || string.IsNullOrWhiteSpace(Password))
					return false;

				PasswordResetTarget t;

				return Enum.TryParse<PasswordResetTarget>(Target, out t);
			} // HasValidParameters

			[UsedImplicitly]
			public int TargetID { get; set; }

			[UsedImplicitly]
			public string Target { get; set; }

			[UsedImplicitly]
			public string Password { get; set; }
		} // class SavePassword

		#endregion class SavePassword

		#endregion private
	} // class ResetPassword123456
} // namespace
