namespace PaymentServices.Calculators
{
	using System;
	using EZBob.DatabaseLib.Model;
	using StructureMap;

	public class SetupFeeCalculator
	{
		private readonly int setupFeeFixed;
		private readonly decimal setupFeePercent;
	    private readonly bool useMax;

		public SetupFeeCalculator()
		{
			var configVariables = ObjectFactory.TryGetInstance<IConfigurationVariablesRepository>();

			setupFeeFixed = configVariables.GetByNameAsInt("SetupFeeFixed");
			setupFeePercent = configVariables.GetByNameAsDecimal("SetupFeePercent");
			useMax = configVariables.GetByNameAsBool("SetupFeeMaxFixedPercent");
		}

		public decimal Calculate(decimal amount)
		{
			if (useMax)
			{
				return Math.Max(Math.Floor(amount * setupFeePercent * 0.01m), setupFeeFixed);
			}

			return Math.Min(Math.Floor(amount * setupFeePercent * 0.01m), setupFeeFixed);
		}
    }
}