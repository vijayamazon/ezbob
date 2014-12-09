namespace EzBob.Backend.Strategies.UserManagement.EmailConfirmation {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class EmailConfirmationConfirmUser : AStrategy {

		public EmailConfirmationConfirmUser(int nUserID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpEmailConfirmationConfirmUser(nUserID, DB, Log);
		} // constructor

		public override string Name {
			get { return "EmailConfirmationConfirmUser"; }
		} // Name

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
		} // Execute

		private readonly SpEmailConfirmationConfirmUser m_oSp;

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

	} // class EmailConfirmationConfirmUser
} // namespace
