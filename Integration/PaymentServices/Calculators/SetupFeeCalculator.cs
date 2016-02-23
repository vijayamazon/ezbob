namespace PaymentServices.Calculators {
	using System;
	using ConfigManager;

	public class SetupFeeCalculator {
		public SetupFeeCalculator(decimal? manualPercent, decimal? brokerPercent) {
			this.setupFeeFixed = CurrentValues.Instance.SetupFeeFixed;
			this.useMax = CurrentValues.Instance.SetupFeeMaxFixedPercent;
			this.brokerSetupFeeRate = CurrentValues.Instance.BrokerSetupFeeRate;

			this.manualPercent = manualPercent ?? 0;
			this.brokerPercent = brokerPercent ?? 0;
		} // constructor

		public struct AbsoluteFeeAmount {
			public decimal Total { get; set; }
			public decimal Broker { get; set; }
		} // AbsoluteFeeAmount

		public AbsoluteFeeAmount Calculate(decimal amount) {
			decimal totalFee = CalculateTotal(amount);

			return new AbsoluteFeeAmount {
				Total = totalFee,
				Broker = CalculateBroker(amount, totalFee),
			};
		} // Calculate

		private decimal CalculateTotal(decimal amount) {
			decimal totalFeePercent = this.manualPercent + this.brokerPercent;

			if (totalFeePercent == 0)
				return 0;

			var totalSetupFee = Math.Floor(amount * totalFeePercent);

			if (this.useMax && totalSetupFee < this.setupFeeFixed)
				return this.setupFeeFixed;

			return totalSetupFee;
		} // CalculateTotal

		private decimal CalculateBroker(decimal amount, decimal totalFee) {
			// No broker fee
			if (this.brokerPercent == 0)
				return 0;

			// Minimum fee - let's have it configurable, but for now make it 100 GBP per loan,
			// of which broker fee will be calculated as 5% of loan amount, the rest is ezbob fee.
			// E.g. on 1000 GBP loan, 5%*1000=50 GBP broker fee, ezbob fee = 100 GBP less 50GBP broker fee = 50 GBP.

			if (totalFee == this.setupFeeFixed) {
				if (this.manualPercent > 0)
					return Math.Floor(amount * this.brokerSetupFeeRate);

				if (this.manualPercent == 0)
					return this.setupFeeFixed;
			} // if

			return Math.Floor(amount * this.brokerPercent);
		} // CalcualteBroker

		private readonly int setupFeeFixed;
		private readonly bool useMax;
		private readonly decimal brokerSetupFeeRate;
		private readonly decimal manualPercent;
		private readonly decimal brokerPercent;
	} // class SetupFeeCalculator
} // namespace
