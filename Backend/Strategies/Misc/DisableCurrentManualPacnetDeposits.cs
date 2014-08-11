namespace EzBob.Backend.Strategies.Misc {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DisableCurrentManualPacnetDeposits : AStrategy
	{
		#region constructor

		public DisableCurrentManualPacnetDeposits(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Disable Current Manual Pacnet Deposits"; }
		} // Name

		#endregion property Name
		
		#region property Execute

		public override void Execute() {
			DB.ExecuteNonQuery("DisableCurrentManualPacnetDeposits", CommandSpecies.StoredProcedure);
			GetAvailableFunds.LoadFromDB();
		} // Execute

		#endregion property Execute
	} // class DisableCurrentManualPacnetDeposits
} // namespace
