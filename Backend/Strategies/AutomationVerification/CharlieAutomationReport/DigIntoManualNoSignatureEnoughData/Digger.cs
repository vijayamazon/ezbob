namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.DigIntoManualNoSignatureEnoughData {
	using Ezbob.Database;
	using Ezbob.Utils;

	using BaseDigger =
		Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData.Digger;

	using BaseCaisAccount =
		Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData.CaisAccount;

	internal class Digger : BaseDigger {
		protected override void LoadPersonalLates() {
			var pc = new ProgressCounter("{0} CAIS accounts loaded.", Log, 20);

			DB.ForEachResult<CaisAccount>(
				ca => {
					if (Result.ContainsKey(ca.CustomerID))
						Result[ca.CustomerID].CaisAccounts.Add(ca);
					else
						Log.Warn("Customer {0} not found for CAIS account.", ca.CustomerID);

					pc.Next();
				},
				BaseCaisAccount.SpName,
				CommandSpecies.StoredProcedure
			);

			pc.Log();
		} // LoadPersonalLates

	} // class Digger
} // namespace
