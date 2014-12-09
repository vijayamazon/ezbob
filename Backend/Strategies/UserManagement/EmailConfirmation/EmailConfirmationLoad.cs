namespace Ezbob.Backend.Strategies.UserManagement.EmailConfirmation {
	using Ezbob.Database;

	public class EmailConfirmationLoad : AStrategy {
		public EmailConfirmationLoad(int nUserID) {
			ErrorMessage = "email confirmation state is not yet loaded";
			m_nUserID = nUserID;
		} // constructor

		public override string Name {
			get { return "EmailConfirmationLoad"; }
		} // Name

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
				"LoadEmailConfirmationState",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserID", m_nUserID)
			);

			if ((int)sr["UserID"] != m_nUserID)
				ErrorMessage = "failed to load email confirmation state";
			else if (!sr["IsConfirmed"])
				ErrorMessage = "email is not confirmed";
			else
				ErrorMessage = null;
		} // Execute

		public bool IsConfirmed {
			get { return string.IsNullOrWhiteSpace(ErrorMessage); }
		} // IsConfirmed

		public string ErrorMessage { get; private set; }

		private readonly int m_nUserID;

	} // class EmailConfirmationLoad
} // namespace
