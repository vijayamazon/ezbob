using System;

namespace EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation
{
	public static class TimeBoundaryCalculationStrategyFactory
	{
		public static ITimeBoundaryCalculationStrategy Create( TimeBoundaryCalculationStrategyType type )
		{
			switch ( type )
			{
				case TimeBoundaryCalculationStrategyType.ByStep:
					return new TimeBoundaryCalculationStrategyByStep();

				case TimeBoundaryCalculationStrategyType.ByEntire:
					return new TimeBoundaryCalculationStrategyByEntire();

				default:
					throw new NotImplementedException();
			}
		}
	}
}