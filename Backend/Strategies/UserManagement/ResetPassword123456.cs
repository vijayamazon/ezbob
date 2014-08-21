namespace EzBob.Backend.Strategies.UserManagement {
	using System.Runtime.Serialization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;
	using MailStrategies;

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
			m_nTargetType = nTarget;

			m_oSpLoad = new LoadEmailForPasswordReset(DB, Log) {
				TargetID = nTargetID,
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
			Log.Debug("Resetting password for user {0}...", m_oSpLoad.TargetID);

			string sEmail = m_oSpLoad.ExecuteScalar<string>();

			if (string.IsNullOrWhiteSpace(sEmail)) {
				Log.Warn("Resetting password for user {0} failed: no email found.", m_oSpLoad.TargetID);
				return;
			} // if

			var sp = new SavePassword(m_oSpLoad, DB, Log) {
				Password = SecurityUtils.HashPassword(sEmail, ThePassword),
			};

			sp.ExecuteNonQuery();

			Log.Debug("Resetting password for user {0} complete.", m_oSpLoad.TargetID);

			AStrategy oEmailSender = null;

			switch (m_nTargetType) {
			case PasswordResetTarget.Customer:
				oEmailSender = new PasswordChanged(m_oSpLoad.TargetID, ThePassword, DB, Log);
				break;

			case PasswordResetTarget.Broker:
				oEmailSender = new BrokerPasswordChanged(m_oSpLoad.TargetID, ThePassword, DB, Log);
				break;
			} // switch

			if (oEmailSender != null)
				oEmailSender.Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly LoadEmailForPasswordReset m_oSpLoad;
		private readonly PasswordResetTarget m_nTargetType;

		private const string ThePassword = "123456";

		#region class LoadEmailForPasswordReset

		private class LoadEmailForPasswordReset : AStoredProcedure {
			public LoadEmailForPasswordReset(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return TargetID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int TargetID { get; set; }
		} // class LoadEmailForPasswordReset

		#endregion class LoadEmailForPasswordReset

		#region class SavePassword

		private class SavePassword : AStoredProcedure {
			public SavePassword(LoadEmailForPasswordReset oSpLoad, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				TargetID = oSpLoad.TargetID;
			} // constructor

			public override bool HasValidParameters() {
				return (TargetID > 0) && !string.IsNullOrWhiteSpace(Password);
			} // HasValidParameters

			[UsedImplicitly]
			public int TargetID { get; set; }

			[UsedImplicitly]
			public string Password { get; set; }
		} // class SavePassword

		#endregion class SavePassword

		#endregion private
	} // class ResetPassword123456
} // namespace
