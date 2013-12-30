namespace EzBob.Backend.Strategies {
	using System;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ValidateMobileCode : AStrategy
	{
		private readonly string mobilePhone;
		private readonly string mobileCode;
		private bool isValidatedSuccessfully;

		#region constructor

		public ValidateMobileCode(string mobilePhone, string mobileCode, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.mobilePhone = mobilePhone;
			this.mobileCode = mobileCode;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Validate mobile code"; }
		} // Name

		#endregion property Name

		#region property Execute

		public override void Execute() {
			DataTable dt = DB.ExecuteReader("ValidateMobileCode", CommandSpecies.StoredProcedure,
				new QueryParameter("Phone", mobilePhone),
				new QueryParameter("Code", mobileCode));

			isValidatedSuccessfully = Convert.ToBoolean(dt.Rows[0]["Success"]);
		} // Execute

		#endregion property Execute

		public bool IsValidatedSuccessfully()
		{
			return isValidatedSuccessfully;
		}
	} // class ValidateMobileCode
} // namespace
