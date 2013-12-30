﻿namespace EzBob.Backend.Strategies {
	using System;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetWizardConfigs : AStrategy
	{
		public bool IsSmsValidationActive { get; set; }
		public int NumberOfMobileCodeAttempts { get; set; }
		#region constructor

		public GetWizardConfigs(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Get wizard configs"; }
		} // Name

		#endregion property Name

		#region property Execute

		public override void Execute() {
			DataTable dt = DB.ExecuteReader("GetWizardConfigs", CommandSpecies.StoredProcedure);
			DataRow row = dt.Rows[0];

			IsSmsValidationActive = Convert.ToBoolean(row["IsSmsValidationActive"]);
			NumberOfMobileCodeAttempts = int.Parse(row["NumberOfMobileCodeAttempts"].ToString());
		} // Execute

		#endregion property Execute
	} // class GenerateMobileCode
} // namespace
