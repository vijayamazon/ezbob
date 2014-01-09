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
		public SetupFeeCalculator(bool setupFee, bool brokerFee)
		{
			var configVariables = ObjectFactory.TryGetInstance<IConfigurationVariablesRepository>();

			_setupFeeFixed = configVariables.GetByNameAsInt("SetupFeeFixed");
			_setupFeePercent = configVariables.GetByNameAsDecimal("SetupFeePercent");
			_useMax = configVariables.GetByNameAsBool("SetupFeeMaxFixedPercent");
			_setupFee = setupFee;
			_brokerFee = brokerFee;
		}

		public decimal Calculate(decimal amount)
		{
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