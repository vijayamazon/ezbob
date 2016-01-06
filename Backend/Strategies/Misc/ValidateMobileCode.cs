namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Database;

	public class ValidateMobileCode : AStrategy {
		public ValidateMobileCode(string sMobilePhone, string sMobileCode) {
			this.mobilePhone = sMobilePhone;
			this.mobileCode = sMobileCode;
			Transaction = null;
		} // constructor

		public override string Name {
			get { return "Validate mobile code"; }
		} // Name

		public ConnectionWrapper Transaction { get; set; }

		public override void Execute() {
			var sr = DB.GetFirst(Transaction, "GetTwilioConfigs", CommandSpecies.StoredProcedure);

			if (!sr.IsEmpty) {
				this.skipCodeGenerationNumber = sr["SkipCodeGenerationNumber"];
				this.skipCodeGenerationNumberCode = sr["SkipCodeGenerationNumberCode"];
			} // if

			if (this.skipCodeGenerationNumber == this.mobilePhone) {
				this.isValidatedSuccessfully = (this.skipCodeGenerationNumberCode == this.mobileCode);
				Log.Info(
					"'Skip code generation' number detected ({0}), code {1} is {2}.",
					this.mobilePhone,
					this.mobileCode,
					IsValidatedSuccessfully() ? "valid" : "invalid"
				);
				return;
			} // if

			this.isValidatedSuccessfully = DB.ExecuteScalar<bool>(
				Transaction,
				"ValidateMobileCode",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Phone", this.mobilePhone),
				new QueryParameter("Code", this.mobileCode)
			);

			Log.Info(
				"For number {0} the code {1} is {2}.",
				this.mobilePhone,
				this.mobileCode,
				IsValidatedSuccessfully() ? "valid" : "invalid"
			);
		} // Execute

		public bool IsValidatedSuccessfully() {
			return this.isValidatedSuccessfully;
		} // IsValidatedSuccessfully

		private readonly string mobilePhone;
		private readonly string mobileCode;
		private bool isValidatedSuccessfully;
		private string skipCodeGenerationNumber;
		private string skipCodeGenerationNumberCode;
	} // class ValidateMobileCode
} // namespace
