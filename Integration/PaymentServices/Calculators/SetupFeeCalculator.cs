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

		public decimal Calculate(decimal amount) {
			decimal totalFeePercent = this.manualPercent + this.brokerPercent;

			if (totalFeePercent == 0)
				return 0;

			var totalSetupFee = Math.Floor(amount * totalFeePercent);

			if (this.useMax && totalSetupFee < this.setupFeeFixed)
				return this.setupFeeFixed;

			return totalSetupFee;
		} // Calculate

		public struct AbsoluteFeeAmount {
			public decimal Total { get; set; }
			public decimal Broker { get; set; }
		} // AbsoluteFeeAmount

		public AbsoluteFeeAmount CalculateTotalAndBroker(decimal amount) {
			var result = new AbsoluteFeeAmount {
				Total = Calculate(amount),
				Broker = 0,
			};

			// No broker fee
			if (this.brokerPercent == 0)
				return result;

			// Minimum fee - let's have it configurable, but for now make it 100 GBP per loan,
			// of which broker fee will be calculated as 5% of loan amount, the rest is ezbob fee.
			// E.g. on 1000 GBP loan, 5%*1000=50 GBP broker fee, ezbob fee = 100 GBP less 50GBP broker fee = 50 GBP.

			if (result.Total == this.setupFeeFixed) {
				if (this.manualPercent > 0) {
					result.Broker = Math.Floor(amount * this.brokerSetupFeeRate);
					return result;
				} // if

				if (this.manualPercent == 0) {
					result.Broker = this.setupFeeFixed;
					return result;
				} // if
			} // if

			result.Broker = Math.Floor(amount * this.brokerPercent);
			return result;
		} // CalculateTotalAndBroker

		private readonly int setupFeeFixed;
		private readonly bool useMax;
		private readonly decimal brokerSetupFeeRate;
		private readonly decimal manualPercent;
		private readonly decimal brokerPercent;
	} // class SetupFeeCalculator
} // namespace
