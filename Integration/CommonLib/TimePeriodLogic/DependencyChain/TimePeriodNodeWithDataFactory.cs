using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain
{
	public class TimePeriodNodeWithDataFactory<T> : ITimePeriodNodeFactory
		where T : class, ITimeRangedData
	{

		public TimePeriodNode Create( TimePeriodEnum timePeriod, ITimeBoundaryCalculationStrategy timeBoundaryCalculateStrategy )
		{
			return new TimePeriodNodeWithData<T>( timePeriod, null, timeBoundaryCalculateStrategy );
		}

		public TimePeriodNode Create( TimePeriodEnum timePeriod, TimePeriodNode child, ITimeBoundaryCalculationStrategy timeBoundaryCalculateStrategy )
		{
			return new TimePeriodNodeWithData<T>( timePeriod, child, timeBoundaryCalculateStrategy );
		}
	}
}