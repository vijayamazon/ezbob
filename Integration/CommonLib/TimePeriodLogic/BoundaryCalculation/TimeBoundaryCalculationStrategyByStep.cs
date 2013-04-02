using System;

namespace EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation
{
	class TimeBoundaryCalculationStrategyByStep : TimeBoundaryCalculationStrategyBase
	{
		public override TimeBoundaryCalculationStrategyType Type
		{
			get { return TimeBoundaryCalculationStrategyType.ByStep; }
		}

		protected override DateTime GetLeftBoundary( ITimePeriod timePeriod, DateTime fromDate )
		{
			return timePeriod.GetLeftStep( fromDate ).AddSeconds( 1 );
		}

		protected override DateTime GetRightBoundary(ITimePeriod timePeriod, DateTime fromDate)
		{
			return fromDate;
		}

		public override int GetCountIncludedMonths( DateTime fromDate, DateTime toDate )
		{
			return fromDate.GetCountIncludedMonthsWithByStep( toDate );
		}
	}
}