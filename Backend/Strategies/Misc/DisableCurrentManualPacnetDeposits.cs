namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Database;

	public class DisableCurrentManualPacnetDeposits : AStrategy {
		public override string Name {
			get { return "Disable Current Manual Pacnet Deposits"; }
		} // Name

		public override void Execute() {
			DB.ExecuteNonQuery("DisableCurrentManualPacnetDeposits", CommandSpecies.StoredProcedure);
			GetAvailableFunds.LoadFromDB();
		} // Execute
	} // class DisableCurrentManualPacnetDeposits
} // namespace
