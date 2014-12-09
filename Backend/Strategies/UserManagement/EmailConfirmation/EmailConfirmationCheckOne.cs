namespace Ezbob.Backend.Strategies.UserManagement.EmailConfirmation {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class EmailConfirmationCheckOne : AStrategy {

		public EmailConfirmationCheckOne(Guid oToken) {
			Response = EmailConfirmationResponse.NotDone;

			m_oSp = new SpEmailConfirmationCheckOne(oToken, DB, Log);
		} // constructor

		public override string Name {
			get { return "EmailConfirmationCheckOne"; }
		} // Name

		public override void Execute() {
			int nResult = m_oSp.ExecuteScalar<int>();

			if ((nResult < (int)EmailConfirmationResponse.Confirmed) || (nResult >= (int)EmailConfirmationResponse.OtherError))
				Response = EmailConfirmationResponse.OtherError;
			else
				Response = (EmailConfirmationResponse)nResult;
		} // Execute

		public EmailConfirmationResponse Response { get; private set; }

		private readonly SpEmailConfirmationCheckOne m_oSp;

		// ReSharper disable ValueParameterNotUsed

		private class SpEmailConfirmationCheckOne : AStoredProc {
			public SpEmailConfirmationCheckOne(Guid oToken, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				Token = oToken;
			} // constructor

			public override bool HasValidParameters() {
				return Token != Guid.Empty;
			} // HasValidParameters() {

			[UsedImplicitly]
			public Guid Token { get; set; }

			[UsedImplicitly]
			public int ExplicitStateID {
				get { return (int)EmailConfirmationRequestState.Confirmed; }
				set { }
			} // ExplicitStateID

			[UsedImplicitly]
			public int ImplicitStateID {
				get { return (int)EmailConfirmationRequestState.ImplicitlyConfirmed; }
				set { }
			} // ImplicitStateID
		} // class SpEmailConfirmationCheckOne

		// ReSharper restore ValueParameterNotUsed

	} // class EmailConfirmationCheckOne
} // namespace
