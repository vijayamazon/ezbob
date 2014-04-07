namespace PaymentServices.Calculators {
	using System;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Loans;
	using StructureMap;

	public class SetupFeeCalculator {
		#region public

		#region constructor

		public SetupFeeCalculator(bool setupFee, bool brokerFee, int? manualAmount, decimal? manualPercent) {
			m_oCfg = ObjectFactory.TryGetInstance<IConfigurationVariablesRepository>();

			_setupFeeFixed = m_oCfg.GetByNameAsInt("SetupFeeFixed");
			_setupFeePercent = m_oCfg.GetByNameAsDecimal("SetupFeePercent");
			_useMax = m_oCfg.GetByNameAsBool("SetupFeeMaxFixedPercent");

			_setupFee = setupFee;
			_brokerFee = brokerFee;
			_manualAmount = manualAmount;
			_manualPercent = manualPercent;
		} // constructor

		#endregion constructor

		#region method Calculate

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

		#endregion method Calculate

		#endregion public

		#region private

		#region method CalculateBroker

		private decimal CalculateBroker(decimal amount) {
			ConfigurationVariable oVar = m_oCfg[ConfigurationVariables.BrokerSetupFeeRate];

			if (oVar.Value.ToUpper() == "TABLE") {
				var brokerFeeRepository = ObjectFactory.GetInstance<BrokerSetupFeeMapRepository>();
				return brokerFeeRepository.GetFee((int)amount);
			} // if

			return amount * (decimal)oVar;
		} // CalculateBroker

		#endregion method CalculateBroker

		private readonly int _setupFeeFixed;
		private readonly decimal _setupFeePercent;
		private readonly bool _useMax;

		private readonly bool _setupFee;
		private readonly bool _brokerFee;
		private readonly int? _manualAmount;
		private readonly decimal? _manualPercent;

		private readonly IConfigurationVariablesRepository m_oCfg;

		#endregion private
	} // class SetupFeeCalculator
} // namespace
