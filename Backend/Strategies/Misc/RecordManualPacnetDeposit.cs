namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Database;

	public class RecordManualPacnetDeposit : AStrategy {
		private readonly string underwriterName;
		private readonly int amount;

		public RecordManualPacnetDeposit(string underwriterName, int amount) {
			this.underwriterName = underwriterName;
			this.amount = amount;
		} // constructor

		public override string Name {
			get { return "Record Manual Pacnet Deposit"; }
		} // Name

		public override void Execute() {
			DB.ExecuteNonQuery("RecordManualPacnetDeposit",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UnderwriterName", underwriterName),
				new QueryParameter("Amount", amount),
				new QueryParameter("Date", DateTime.UtcNow)
			);

            var availFunds = new GetAvailableFunds(); //fix for static log and db init
			GetAvailableFunds.LoadFromDB();
		} // Execute

	} // class RecordManualPacnetDeposit
} // namespace
