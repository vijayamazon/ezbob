namespace EzBob.Backend.Strategies {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ValidateMobileCode : AStrategy {
		#region public

		#region constructor

		public ValidateMobileCode(string sMobilePhone, string sMobileCode, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
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

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Validate mobile code"; }
		} // Name

		#endregion property Name

		#region method Execute

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

		#endregion method Execute

		#region method IsValidatedSuccessfully

		public bool IsValidatedSuccessfully() {
			return m_bIsValidatedSuccessfully;
		} // IsValidatedSuccessfully

		#endregion method IsValidatedSuccessfully

		#endregion public

		#region private

		private readonly string m_sMobilePhone;
		private readonly string m_sMobileCode;
		private bool m_bIsValidatedSuccessfully;
		private string m_sSkipCodeGenerationNumber;
		private string m_sSkipCodeGenerationNumberCode;

		#endregion private
	} // class ValidateMobileCode
} // namespace
