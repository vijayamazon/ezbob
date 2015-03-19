namespace PaymentServices.Calculators {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Loans;
	using StructureMap;

	public class SetupFeeCalculator {
		public static void ReloadBrokerRepoCache() {
			brokerRepoCache = new List<BrokerRepoCached>();

			var brokerFeeRepository = ObjectFactory.GetInstance<BrokerSetupFeeMapRepository>();

			foreach (BrokerSetupFeeMap x in brokerFeeRepository.GetAll())
				brokerRepoCache.Add(new BrokerRepoCached(x));
		} // ReloadBrokerCache

		[SuppressMessage("ReSharper", "RedundantDelegateCreation")]
		public SetupFeeCalculator(bool setupFee, bool brokerFee, int? manualAmount, decimal? manualPercent) {
			this.setupFeeFixed = CurrentValues.Instance.SetupFeeFixed;
			this.setupFeePercent = CurrentValues.Instance.SetupFeePercent;

			this.defaultFeeSelector = CurrentValues.Instance.SetupFeeMaxFixedPercent
				? new Func<decimal, decimal, decimal>(Math.Max)
				: new Func<decimal, decimal, decimal>(Math.Min);

			this.setupFee = setupFee;
			this.brokerFee = brokerFee;
			this.manualAmount = manualAmount;
			this.manualPercent = manualPercent;
		} // constructor

		public decimal Calculate(decimal amount, bool useBrokerCache = false) {
			// Use manual fee.
			if (this.setupFee || this.brokerFee) {
				bool hasManual =
					(this.manualAmount.HasValue && this.manualAmount.Value > 0) ||
					(this.manualPercent.HasValue && this.manualPercent.Value > 0);

				if (hasManual) {
					return Math.Max(
						Math.Floor(amount * (this.manualPercent ?? 0M)),
						this.manualAmount ?? 0
					);
				} // if
			} // if

			if (this.brokerFee) // Use broker fee.
				return CalculateBroker(amount, useBrokerCache);
			else if (this.setupFee) // Use default fee.
				return this.defaultFeeSelector(Math.Floor(amount * this.setupFeePercent * 0.01m), this.setupFeeFixed);
			else // Don't use fee.
				return 0M;
		} // Calculate

		private decimal CalculateBroker(decimal amount, bool useBrokerCache) {
			VariableValue oVar = CurrentValues.Instance.BrokerSetupFeeRate;

			if (oVar.Value.ToUpper() == "TABLE") {
				if (useBrokerCache) {
					if (brokerRepoCache == null)
						return 0;

					var cached = brokerRepoCache.FirstOrDefault(x => x.Matches((int)amount));

					return cached == null ? 0 : cached.Fee;
				} else {
					var brokerFeeRepository = ObjectFactory.GetInstance<BrokerSetupFeeMapRepository>();
					return brokerFeeRepository.GetFee((int)amount);
				} // if
			} // if

			// ReSharper disable once RedundantCast
			return amount * (decimal)oVar;
		} // CalculateBroker

		private class BrokerRepoCached {
			public BrokerRepoCached(BrokerSetupFeeMap map) {
				this.minAmount = map.MinAmount;
				this.maxAmount = map.MaxAmount;
				Fee = map.Fee;
			} // constructor

			public bool Matches(int amount) {
				return this.minAmount <= amount && amount <= this.maxAmount;
			} // Matches

			public int Fee { get; set; }

			private readonly int minAmount;
			private readonly int maxAmount;
		} // class BrokerRepoCached

		private static List<BrokerRepoCached> brokerRepoCache;

		private readonly int setupFeeFixed;
		private readonly decimal setupFeePercent;
		private readonly Func<decimal, decimal, decimal> defaultFeeSelector;
		private readonly bool setupFee;
		private readonly bool brokerFee;
		private readonly int? manualAmount;
		private readonly decimal? manualPercent;
	} // class SetupFeeCalculator
} // namespace
