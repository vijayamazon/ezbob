namespace EzBob.Backend.Strategies.UserManagement.EmailConfirmation {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class EmailConfirmationGenerate : AStrategy {

		public EmailConfirmationGenerate(int nUserID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Address = string.Empty;

			m_oSp = new SpEmailConfirmationGenerate(nUserID, DB, Log);
		} // constructor

		public override string Name {
			get { return "EmailConfirmationGenerate"; }
		} // Name

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
			Address = string.Format("<a href='{0}/confirm/{1}'>click here</a>", CustomerSite, Token);

			Log.Debug("Confirmation token {0} has been created for user {1}.", Token.ToString("N"), m_oSp.UserID);
		} // Execute

		public Guid Token { get { return m_oSp.Token; } } // Token

		public string Address { get; private set; }

		private readonly SpEmailConfirmationGenerate m_oSp;

		// ReSharper disable ValueParameterNotUsed

		private class SpEmailConfirmationGenerate : AStoredProc {
			public SpEmailConfirmationGenerate(int nUserID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				m_oToken = Guid.NewGuid();
				UserID = nUserID;
			} // constructor

			public override bool HasValidParameters() {
				return UserID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public Guid Token {
				get { return m_oToken; }
				set { }
			} // Token

			private readonly Guid m_oToken;

			[UsedImplicitly]
			public int UserID { get; set; }

			[UsedImplicitly]
			public int EmailStateID {
				get { return (int)EmailConfirmationRequestState.Pending; }
				set { }
			} // EmailStateID

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				set { }
			} // Now
		} // class SpEmailConfirmationGenerate

		// ReSharper restore ValueParameterNotUsed

	} // class EmailConfirmationGenerate
} // namespace
