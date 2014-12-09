namespace PaymentServices.Calculators {
	using System;
	using ConfigManager;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Loans;
	using StructureMap;

	public class SetupFeeCalculator {

		public SetupFeeCalculator(bool setupFee, bool brokerFee, int? manualAmount, decimal? manualPercent) {
			_setupFeeFixed = CurrentValues.Instance.SetupFeeFixed;
			_setupFeePercent = CurrentValues.Instance.SetupFeePercent;
			_useMax = CurrentValues.Instance.SetupFeeMaxFixedPercent;

			_setupFee = setupFee;
			_brokerFee = brokerFee;
			_manualAmount = manualAmount;
			_manualPercent = manualPercent;
		} // constructor

		public decimal Calculate(decimal amount) {
			//use manual fee
			if (_setupFee || _brokerFee) {
				if ((_manualAmount.HasValue && _manualAmount.Value > 0) || (_manualPercent.HasValue && _manualPercent.Value > 0)) {
					return Math.Max(Math.Floor(amount * (_manualPercent.HasValue ? _manualPercent.Value : 0M)),
						_manualAmount.HasValue ? _manualAmount.Value : 0);
				}
			} // if

			//use broker fee
			if (_brokerFee)
				return CalculateBroker(amount);

			//use default fee
			if (_setupFee) {
				if (_useMax)
					return Math.Max(Math.Floor(amount * _setupFeePercent * 0.01m), _setupFeeFixed);

				return Math.Min(Math.Floor(amount * _setupFeePercent * 0.01m), _setupFeeFixed);
			} // if

			//don't use fee
			return 0M;
		} // Calculate

		private decimal CalculateBroker(decimal amount) {
			var oVar = CurrentValues.Instance.BrokerSetupFeeRate;

			if (oVar.Value.ToUpper() == "TABLE") {
				var brokerFeeRepository = ObjectFactory.GetInstance<BrokerSetupFeeMapRepository>();
				return brokerFeeRepository.GetFee((int)amount);
			} // if

			return amount * (decimal)oVar;
		} // CalculateBroker

		private readonly int _setupFeeFixed;
		private readonly decimal _setupFeePercent;
		private readonly bool _useMax;

		private readonly bool _setupFee;
		private readonly bool _brokerFee;
		private readonly int? _manualAmount;
		private readonly decimal? _manualPercent;

	} // class SetupFeeCalculator
} // namespace
