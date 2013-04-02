using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain
{
	public class TimePeriodNodeFactory : ITimePeriodNodeFactory
	{
		private TimePeriodNode Create( TimePeriodEnum timePeriod, TimePeriodNode child = null )
		{
			return new TimePeriodNode( timePeriod, child );
		}

		public TimePeriodNode Create(TimePeriodEnum timePeriod, ITimeBoundaryCalculationStrategy timeBoundaryCalculateStrategy = null)
		{
			return Create( timePeriod, null, timeBoundaryCalculateStrategy );
		}

		public TimePeriodNode Create(TimePeriodEnum timePeriod, TimePeriodNode child, ITimeBoundaryCalculationStrategy timeBoundaryCalculateStrategy = null)
		{
			return Create( timePeriod, child );
		}
	}
}