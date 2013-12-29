namespace EzBob.Backend.Strategies {
	using System;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GenerateMobileCode : AStrategy
	{
		private readonly string mobilePhone;
		private string code;

		#region constructor

		public GenerateMobileCode(string mobilePhone, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.mobilePhone = mobilePhone;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Generate mobile code"; }
		} // Name

		#endregion property Name

		#region property Execute

		public override void Execute() {
			var random = new Random();
			code = (100000 + random.Next(899999)).ToString(CultureInfo.InvariantCulture);

			DB.ExecuteNonQuery("StoreMobileCode", CommandSpecies.StoredProcedure,
				new QueryParameter("MobilePhone", mobilePhone),
				new QueryParameter("Code", code));
		} // Execute

		#endregion property Execute

		public string GetCode()
		{
			return code;
		}
	} // class GenerateMobileCode
} // namespace
