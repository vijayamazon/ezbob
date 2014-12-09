namespace EzBob.Backend.Strategies.Misc {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DisableCurrentManualPacnetDeposits : AStrategy
	{

		public DisableCurrentManualPacnetDeposits(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		} // constructor

		public override string Name {
			get { return "Disable Current Manual Pacnet Deposits"; }
		} // Name

		public override void Execute() {
			DB.ExecuteNonQuery("DisableCurrentManualPacnetDeposits", CommandSpecies.StoredProcedure);
			GetAvailableFunds.LoadFromDB();
		} // Execute

	} // class DisableCurrentManualPacnetDeposits
} // namespace
