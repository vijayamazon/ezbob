namespace Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport.DigIntoManualNoSignatureEnoughData {
	using BaseCaisAccount =
		Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData.CaisAccount;

	internal class CaisAccount : BaseCaisAccount {
		public override bool IsLateForApprove {
			get {
				return CarCaisAccount.IsBad(DecisionTime, LastUpdatedDate, Balance, AccountStatusCodes);
			} // get
		} // IsLateForApprove
	} // class CaisAccount
} // namespace
