namespace Ezbob.Backend.Strategies.MedalCalculations {
	using System;
	using Ezbob.Database;

	// This strategy assumes that the table MedalCalculations is empty
	// It will go over all the customers in the DB and fill the medal for them
	public class Temp_BackFillMedals : AStrategy {
		public override string Name {
			get { return "Temp_BackFillMedals"; }
		}

		public override void Execute() {
			DB.ForEachRowSafe(
				sr => {
					int customerId = sr["Id"];

					Log.Info("Will calculate medal for customer:{0}", customerId);

					try {
						var instance = new CalculateMedal(customerId, null, null, DateTime.UtcNow, false, true);
						instance.Execute();
					} catch (Exception e) {
						Log.Error("Exception during medal calculation for customer:{0} The exception:{1}", customerId, e);
					}
				},
				"SELECT Id FROM Customer WHERE IsTest = 0 AND WizardStep = 4 ORDER BY Id DESC",
				CommandSpecies.Text
			);
		}
	}
}
