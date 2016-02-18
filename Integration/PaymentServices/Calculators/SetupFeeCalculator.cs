namespace PaymentServices.Calculators {
	using System;
	using ConfigManager;

	public class SetupFeeCalculator {
		public SetupFeeCalculator(decimal? manualPercent, decimal? brokerPercent) {
			this.setupFeeFixed = CurrentValues.Instance.SetupFeeFixed;
			this.useMax = CurrentValues.Instance.SetupFeeMaxFixedPercent;
			this.brokerSetupFeeRate = CurrentValues.Instance.BrokerSetupFeeRate;

			this.manualPercent = manualPercent;
			this.brokerPercent = brokerPercent;
		} // constructor

		public decimal Calculate(decimal amount) {
			decimal totalFeePercent = (this.manualPercent ?? 0) + (this.brokerPercent ?? 0);

			if (totalFeePercent == 0)
				return 0;

			var totalSetupFee = Math.Floor(amount * totalFeePercent);

			if (this.useMax && totalSetupFee < this.setupFeeFixed)
				return this.setupFeeFixed;

			return totalSetupFee;
		} // Calculate

		public decimal CalculateBrokerFee(decimal amount) {
			// No broker fee
			if (!this.brokerPercent.HasValue || this.brokerPercent.Value == 0)
				return 0M;

			decimal totalFee = Calculate(amount);

			// Minimum fee - let's have it configurable, but for now make it 100 GBP per loan,
			// of which broker fee will be calculated as 5% of loan amount, the rest is ezbob fee. 
			// e.g. on 1000 GBP loan, 5%*1000=50 GBP broker fee, ezbob fee = 100 GBP less 50GBP broker fee = 50 GBP.
			if (totalFee == this.setupFeeFixed && this.manualPercent.HasValue && this.manualPercent.Value > 0)
				return Math.Floor(amount * this.brokerSetupFeeRate);

			if (totalFee == this.setupFeeFixed && (!this.manualPercent.HasValue || this.manualPercent.Value == 0))
				return this.setupFeeFixed;

			return Math.Floor(amount * this.brokerPercent.Value);
		} // CalculateBrokerFee

		private readonly int setupFeeFixed;
		private readonly bool useMax;
		private readonly decimal brokerSetupFeeRate;
		private readonly decimal? manualPercent;
		private readonly decimal? brokerPercent;
	} // class SetupFeeCalculator
} // namespace
