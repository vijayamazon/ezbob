namespace EzBob.Models {
	using System;
	using ConfigManager;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Loans;

	[Serializable]
	public class LoanChargesModel : LoanCharge {
		public string IsServiceFee { get; private set; }

		public LoanChargesModel() {
			IsServiceFee = string.Empty;
		} // constructor

		public static LoanChargesModel FromCharges(LoanCharge loanCharge) {
			if (loanCharge == null)
				return null;

			return new LoanChargesModel {
				Amount = loanCharge.Amount,
				ChargesType = ConfigurationVariableModel.FromConfigurationVariable(loanCharge.ChargesType),
				Date = loanCharge.Date,
				Id = loanCharge.Id,
				State = loanCharge.State ?? "Active",
				AmountPaid = loanCharge.AmountPaid,
				Description = loanCharge.GetDescription(),
				IsServiceFee = loanCharge.ChargesType.Name == spreadSetupFee.Name ? "yes" : "no",
			};
		} // FromCharges

		private static readonly ConfigurationVariable spreadSetupFee =
			new ConfigurationVariable(CurrentValues.Instance.SpreadSetupFeeCharge);
	} // class LoanChargesModel
} // namespace
