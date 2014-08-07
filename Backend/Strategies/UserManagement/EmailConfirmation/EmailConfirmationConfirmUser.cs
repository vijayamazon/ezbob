namespace EzBob.Backend.Strategies.UserManagement.EmailConfirmation {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class EmailConfirmationConfirmUser : AStrategy {
		#region public

		#region constructor

		public EmailConfirmationConfirmUser(int nUserID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpEmailConfirmationConfirmUser(nUserID, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "EmailConfirmationConfirmUser"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpEmailConfirmationConfirmUser m_oSp;

		#region class SpEmailConfirmationConfirmUser
		// ReSharper disable ValueParameterNotUsed

		private class SpEmailConfirmationConfirmUser : AStoredProc {
			public SpEmailConfirmationConfirmUser(int nUserID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				UserID = nUserID;
			} // constructor

			public override bool HasValidParameters() {
				return UserID > 0;
			} // HasValidParameters() {

			[UsedImplicitly]
			public int UserID { get; set; }

			[UsedImplicitly]
			public int EmailStateID {
				get { return (int)EmailConfirmationRequestState.ManuallyConfirmed; }
				set { }
			} // ExplicitStateID
		} // class SpEmailConfirmationConfirmUser

		// ReSharper restore ValueParameterNotUsed
		#endregion class SpEmailConfirmationConfirmUser

		#endregion private
	} // class EmailConfirmationConfirmUser
} // namespace
