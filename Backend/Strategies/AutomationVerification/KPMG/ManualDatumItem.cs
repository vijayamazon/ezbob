namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using PaymentServices.Calculators;

	internal class ManualDatumItem : ADatumItem {
		public ManualDatumItem(SpLoadCashRequestsForAutomationReport.ResultRow sr) {
			sr.CopyTo(this);
		} // constructor

		public override string DecisionStr {
			get { return IsApproved ? "Approved" : "Rejected"; }
		} // DecisionStr

		public override bool IsAuto { get { return false; } }

		public virtual void Calculate() {
			SetupFeeAmount = new SetupFeeCalculator(
				UseSetupFee == 1,
				UseBrokerSetupFee,
				ManualSetupFeeAmount,
				ManualSetupFeePercent
			).Calculate(ApprovedAmount, true);

			SetupFeePct = ApprovedAmount <= 0 ? 0 : SetupFeeAmount / ApprovedAmount;
		} // Calculate
	} // ManualDatumItem
} // namespace
