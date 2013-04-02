using System;

namespace EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation
{
	public abstract class TimeBoundaryCalculationStrategyBase: ITimeBoundaryCalculationStrategy
	{
		public abstract TimeBoundaryCalculationStrategyType Type { get; }
		
		protected abstract DateTime GetLeftBoundary( ITimePeriod timePeriod, DateTime fromDate );
		protected abstract DateTime GetRightBoundary( ITimePeriod timePeriod, DateTime fromDate );

		public DateTime GetRightBoundary(TimePeriodEnum timePeriodType, DateTime fromDate)
		{
			return GetRightBoundary( TimePeriodFactory.Create( timePeriodType ), fromDate );
		}

		public abstract int GetCountIncludedMonths(DateTime fromDate, DateTime toDate);

		public DateTime GetLeftBoundary( TimePeriodEnum timePeriodType, DateTime fromDate )
		{
			return GetLeftBoundary( TimePeriodFactory.Create( timePeriodType ), fromDate );
		}

	}
}