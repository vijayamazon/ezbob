namespace EzBob.Backend.Strategies.UserManagement.EmailConfirmation {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class EmailConfirmationGenerate : AStrategy {
		#region public

		#region constructor

		public EmailConfirmationGenerate(int nUserID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Address = string.Empty;

			m_oSp = new SpEmailConfirmationGenerate(nUserID, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "EmailConfirmationGenerate"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
			Address = string.Format("<a href='{0}/confirm/{1}'>click here</a>", CurrentValues.Instance.CustomerSite.Value, Token);

			Log.Debug("Confirmation token {0} has been created for user {1}.", Token.ToString("N"), m_oSp.UserID);
		} // Execute

		#endregion method Execute

		#region property Token

		public Guid Token { get { return m_oSp.Token; } } // Token

		#endregion property Token

		#region property Address

		public string Address { get; private set; }

		#endregion property Address

		#endregion public

		#region private

		private readonly SpEmailConfirmationGenerate m_oSp;

		#region class SpEmailConfirmationGenerate
		// ReSharper disable ValueParameterNotUsed

		private class SpEmailConfirmationGenerate : AStoredProc {
			public SpEmailConfirmationGenerate(int nUserID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				m_oToken = Guid.NewGuid();
				UserID = nUserID;
			} // constructor

			public override bool HasValidParameters() {
				return UserID > 0;
			} // HasValidParameters

			#region property Token

			[UsedImplicitly]
			public Guid Token {
				get { return m_oToken; }
				set { }
			} // Token

			private readonly Guid m_oToken;

			#endregion property Token

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
		#endregion class SpEmailConfirmationGenerate

		#endregion private
	} // class EmailConfirmationGenerate
} // namespace
