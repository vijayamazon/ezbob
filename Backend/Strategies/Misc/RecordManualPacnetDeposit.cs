namespace EzBob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class RecordManualPacnetDeposit : AStrategy
	{
		private readonly string underwriterName;
		private readonly int amount;
		#region constructor

		public RecordManualPacnetDeposit(string underwriterName, int amount, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.underwriterName = underwriterName;
			this.amount = amount;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Record Manual Pacnet Deposit"; }
		} // Name

		#endregion property Name
		
		#region property Execute

		public override void Execute() {
			DB.ExecuteNonQuery("RecordManualPacnetDeposit",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UnderwriterName", underwriterName),
				new QueryParameter("Amount", amount),
				new QueryParameter("Date", DateTime.UtcNow)
			);

			GetAvailableFunds.LoadFromDB();
		} // Execute

		#endregion property Execute
	} // class RecordManualPacnetDeposit
} // namespace
