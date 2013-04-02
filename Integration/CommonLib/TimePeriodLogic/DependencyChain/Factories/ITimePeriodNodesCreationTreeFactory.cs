using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain.Factories
{
	public class TimePeriodNodesCreationTreeFactoryFactory
	{
		public static ITimePeriodNodesCreationTreeFactory CreateHardCodeTimeBoundaryCalculationStrategy()
		{
			return new TimePeriodNodesCreationTreeFactoryHardCodeTimeBoundaryCalculationStrategy();
		}

		public static ITimePeriodNodesCreationTreeFactory CreateSameTimeBoundaryCalculationStrategy(ITimeBoundaryCalculationStrategy strategy)
		{
			return new TimePeriodNodesCreationTreeFactorySameTimeBoundaryCalculationStrategy(strategy);
		}

		public static ITimePeriodNodesCreationTreeFactory CreateSameTimeBoundaryCalculationStrategy( TimeBoundaryCalculationStrategyType strategyType )
		{
			var strategy = TimeBoundaryCalculationStrategyFactory.Create( strategyType );
			return new TimePeriodNodesCreationTreeFactorySameTimeBoundaryCalculationStrategy( strategy );
		}
	}

	public enum TimePeriodNodesCreationTreeFactoryType
	{
		SameTimeBoundaryCalculationStrategy,
		HardCodeTimeBoundaryCalculationStrategy
	}

	public interface ITimePeriodNodesCreationTreeFactory
	{
		TimePeriodNodesCreationTreeFactoryType Type { get; }
		TimePeriodNode Create( ITimePeriodNodeFactory factory );
	}
}
