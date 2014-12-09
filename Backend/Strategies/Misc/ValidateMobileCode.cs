namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Database;

	public class ValidateMobileCode : AStrategy {

		public ValidateMobileCode(string sMobilePhone, string sMobileCode) {
			m_sMobilePhone = sMobilePhone;
			m_sMobileCode = sMobileCode;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_sSkipCodeGenerationNumber = sr["SkipCodeGenerationNumber"];
					m_sSkipCodeGenerationNumberCode = sr["SkipCodeGenerationNumberCode"];

					return ActionResult.SkipAll;
				},
				"GetTwilioConfigs",
				CommandSpecies.StoredProcedure
			);
		} // constructor

		public override string Name {
			get { return "Validate mobile code"; }
		} // Name

		public override void Execute() {
			if (m_sSkipCodeGenerationNumber == m_sMobilePhone) {
				m_bIsValidatedSuccessfully = (m_sSkipCodeGenerationNumberCode == m_sMobileCode);
				Log.Info(
					"'Skip code generation' number detected ({0}), code {1} is {2}.",
					m_sMobilePhone,
					m_sMobileCode,
					IsValidatedSuccessfully() ? "valid" : "invalid"
				);
				return;
			} // if

			m_bIsValidatedSuccessfully = DB.ExecuteScalar<bool>(
				"ValidateMobileCode",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Phone", m_sMobilePhone),
				new QueryParameter("Code", m_sMobileCode)
			);

			Log.Info(
				"For number {0} the code {1} is {2}.",
				m_sMobilePhone,
				m_sMobileCode,
				IsValidatedSuccessfully() ? "valid" : "invalid"
			);
		} // Execute

		public bool IsValidatedSuccessfully() {
			return m_bIsValidatedSuccessfully;
		} // IsValidatedSuccessfully

		private readonly string m_sMobilePhone;
		private readonly string m_sMobileCode;
		private bool m_bIsValidatedSuccessfully;
		private string m_sSkipCodeGenerationNumber;
		private string m_sSkipCodeGenerationNumberCode;

	} // class ValidateMobileCode
} // namespace
