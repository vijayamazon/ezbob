namespace PaymentServices.Calculators
{
	using System;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Loans;
	using StructureMap;

	public class SetupFeeCalculator
	{
		private readonly int _setupFeeFixed;
		private readonly decimal _setupFeePercent;
	    private readonly bool _useMax;

		private readonly bool _setupFee;
		private readonly bool _brokerFee;
		private readonly int? _manualAmount;
		private readonly decimal? _manualPercent;
		public SetupFeeCalculator(bool setupFee, bool brokerFee, int? manualAmount, decimal? manualPercent)
		{
			var configVariables = ObjectFactory.TryGetInstance<IConfigurationVariablesRepository>();

			_setupFeeFixed = configVariables.GetByNameAsInt("SetupFeeFixed");
			_setupFeePercent = configVariables.GetByNameAsDecimal("SetupFeePercent");
			_useMax = configVariables.GetByNameAsBool("SetupFeeMaxFixedPercent");
			_setupFee = setupFee;
			_brokerFee = brokerFee;
			_manualAmount = manualAmount;
			_manualPercent = manualPercent;
		}

		public decimal Calculate(decimal amount)
		{
			if (_manualAmount.HasValue || _manualPercent.HasValue)
			{
				return Math.Max(Math.Floor(amount*(_manualPercent.HasValue ? _manualPercent.Value : 0M)*0.01M),
				                _manualAmount.HasValue ? _manualAmount.Value : 0);
			}

			if (_brokerFee)
			{
				return CalculateBroker(amount);
			}
			if (_setupFee)
			{
				if (_useMax)
				{
					return Math.Max(Math.Floor(amount*_setupFeePercent*0.01m), _setupFeeFixed);
				}

				return Math.Min(Math.Floor(amount*_setupFeePercent*0.01m), _setupFeeFixed);
			}
			return 0M;
		}

		private decimal CalculateBroker(decimal amount)
		{
			var brokerFeeRepository = ObjectFactory.GetInstance<BrokerSetupFeeMapRepository>();
			return brokerFeeRepository.GetFee((int)amount);
		}

		
	}
}