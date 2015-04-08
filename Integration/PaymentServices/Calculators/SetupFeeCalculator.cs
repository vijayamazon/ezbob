namespace PaymentServices.Calculators {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Loans;
	using StructureMap;

    public class SetupFeeCalculator {

		public SetupFeeCalculator(decimal? manualPercent, decimal? brokerPercent) {
			this.setupFeeFixed = CurrentValues.Instance.SetupFeeFixed;
			this.useMax = CurrentValues.Instance.SetupFeeMaxFixedPercent;
            this.brokerSetupFeeRate = CurrentValues.Instance.BrokerSetupFeeRate;

			this.manualPercent = manualPercent;
		    this.brokerPercent = brokerPercent;
		} // constructor

        public decimal Calculate(decimal amount) {
			decimal? totalFeePercent = this.manualPercent + this.brokerPercent;

		    if (totalFeePercent.HasValue) {
                var totalSetupFee = Math.Floor(amount * totalFeePercent.Value);

                if (this.useMax && totalSetupFee < this.setupFeeFixed) {
                    return this.setupFeeFixed;
                }

                return totalSetupFee;
            }

		    return 0M;
		} // Calculate

		public decimal CalculateBrokerFee(decimal amount) {
            //No broker fee
            if (!this.brokerPercent.HasValue) {
                return 0M;
            }

		    decimal totalFee = Calculate(amount);

            //1. Minimum fee - let's have it configurable, but for now make it 100 GBP per loan, of which broker fee will be calculated as 5% of loan amount, the rest is ezbob fee. 
            //e.g. on 1000 GBP loan, 5%*1000=50 GBP broker fee, ezbob fee = 100 GBP less 50GBP broker fee = 50 GBP.
            if (totalFee == this.setupFeeFixed) {
                return Math.Floor(amount * this.brokerSetupFeeRate);
            }

		    return Math.Floor(amount * this.brokerPercent.Value);
		} // CalculateBrokerFee

		private readonly int setupFeeFixed;
		private readonly bool useMax;
	    private readonly decimal brokerSetupFeeRate;
        private readonly decimal? manualPercent;
        private readonly decimal? brokerPercent;

	} // class SetupFeeCalculator
} // namespace
