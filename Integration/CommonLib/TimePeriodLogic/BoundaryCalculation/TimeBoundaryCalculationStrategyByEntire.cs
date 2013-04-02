using System;

namespace EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation
{
	class TimeBoundaryCalculationStrategyByEntire : TimeBoundaryCalculationStrategyBase
	{
		public override TimeBoundaryCalculationStrategyType Type
		{
			get { return TimeBoundaryCalculationStrategyType.ByEntire; }
		}

		protected override DateTime GetLeftBoundary( ITimePeriod timePeriod, DateTime fromDate )
		{
			return timePeriod.GetLeftBoundary( fromDate );
		}

		protected override DateTime GetRightBoundary(ITimePeriod timePeriod, DateTime fromDate)
		{
			return timePeriod.GetRightBoundary( fromDate );
		}

		public override int GetCountIncludedMonths( DateTime fromDate, DateTime toDate )
		{
			return fromDate.GetCountIncludedMonthsWithByEntire( toDate );
		}

		
	}
}