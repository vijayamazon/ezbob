using System;

namespace EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation
{
	public interface ITimeBoundaryCalculationStrategy
	{
		TimeBoundaryCalculationStrategyType Type { get; }
		DateTime GetLeftBoundary( TimePeriodEnum timePeriodType, DateTime fromDate );
		DateTime GetRightBoundary( TimePeriodEnum timePeriodType, DateTime fromDate );
		int GetCountIncludedMonths( DateTime fromDate, DateTime toDate );
	}

	public enum TimeBoundaryCalculationStrategyType
	{
		ByStep,
		ByEntire
	}
}