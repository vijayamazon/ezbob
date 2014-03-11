namespace EzBob.Backend.Strategies {
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
			var sr = new SafeReader(dt.Rows[0]);

			IsSmsValidationActive = sr["IsSmsValidationActive"];
			NumberOfMobileCodeAttempts = sr["NumberOfMobileCodeAttempts"];
		} // Execute

		#endregion property Execute
	} // class GetWizardConfigs
} // namespace
