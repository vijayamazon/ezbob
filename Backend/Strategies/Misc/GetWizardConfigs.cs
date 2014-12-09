namespace EzBob.Backend.Strategies.Misc {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetWizardConfigs : AStrategy
	{
		public bool IsSmsValidationActive { get; set; }
		public int NumberOfMobileCodeAttempts { get; set; }

		public GetWizardConfigs(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		} // constructor

		public override string Name {
			get { return "Get wizard configs"; }
		} // Name

		public override void Execute() {
			SafeReader sr = DB.GetFirst("GetWizardConfigs", CommandSpecies.StoredProcedure);

			IsSmsValidationActive = sr["IsSmsValidationActive"];
			NumberOfMobileCodeAttempts = sr["NumberOfMobileCodeAttempts"];
		} // Execute

	} // class GetWizardConfigs
} // namespace
