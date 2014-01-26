namespace EzBob.Backend.Strategies {
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ValidateMobileCode : AStrategy
	{
		private readonly string mobilePhone;
		private readonly string mobileCode;
		private bool isValidatedSuccessfully;
		private string skipCodeGenerationNumber;
		private string skipCodeGenerationNumberCode;

		#region constructor

		public ValidateMobileCode(string mobilePhone, string mobileCode, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.mobilePhone = mobilePhone;
			this.mobileCode = mobileCode;
			ReadConfigurations();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Validate mobile code"; }
		} // Name

		#endregion property Name

		#region property Execute

		public override void Execute() 
		{
			if (skipCodeGenerationNumber == mobilePhone && skipCodeGenerationNumberCode == mobileCode)
			{
				Log.Info("Mobile phone {0} detected. Validation of code won't be done.", mobilePhone);
				isValidatedSuccessfully = true;
				return;
			}

			DataTable dt = DB.ExecuteReader("ValidateMobileCode", CommandSpecies.StoredProcedure,
				new QueryParameter("Phone", mobilePhone),
				new QueryParameter("Code", mobileCode));

			var sr = new SafeReader(dt.Rows[0]);
			isValidatedSuccessfully = sr["Success"];
		} // Execute

		#endregion property Execute

		private void ReadConfigurations()
		{
			DataTable dt = DB.ExecuteReader("GetTwilioConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];
			var sr = new SafeReader(results);
			skipCodeGenerationNumber = sr["SkipCodeGenerationNumber"];
			skipCodeGenerationNumberCode = sr["SkipCodeGenerationNumberCode"];
		}

		public bool IsValidatedSuccessfully()
		{
			return isValidatedSuccessfully;
		}
	} // class ValidateMobileCode
} // namespace
