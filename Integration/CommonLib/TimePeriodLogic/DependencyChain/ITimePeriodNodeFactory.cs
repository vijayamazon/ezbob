using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain
{
	public interface ITimePeriodNodeFactory
	{
		TimePeriodNode Create( TimePeriodEnum timePeriod, ITimeBoundaryCalculationStrategy timeBoundaryCalculateStrategy );
		TimePeriodNode Create( TimePeriodEnum timePeriod, TimePeriodNode child, ITimeBoundaryCalculationStrategy timeBoundaryCalculateStrategy );		
	}
}