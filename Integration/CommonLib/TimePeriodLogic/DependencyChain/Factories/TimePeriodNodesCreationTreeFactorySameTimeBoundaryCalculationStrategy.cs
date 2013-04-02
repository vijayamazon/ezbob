using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain.Factories
{
	class TimePeriodNodesCreationTreeFactorySameTimeBoundaryCalculationStrategy : ITimePeriodNodesCreationTreeFactory
	{
		private readonly ITimeBoundaryCalculationStrategy _TimeBoundaryCalculateStrategy;

		public TimePeriodNodesCreationTreeFactorySameTimeBoundaryCalculationStrategy( ITimeBoundaryCalculationStrategy timeBoundaryCalculateStrategy )
		{
			_TimeBoundaryCalculateStrategy = timeBoundaryCalculateStrategy;
		}

		public TimePeriodNodesCreationTreeFactoryType Type
		{
			get { return TimePeriodNodesCreationTreeFactoryType.SameTimeBoundaryCalculationStrategy; }
		}

		public TimePeriodNode Create( ITimePeriodNodeFactory factory )
		{
			var lastLeaf = factory.Create( TimePeriodEnum.Month, _TimeBoundaryCalculateStrategy );
			lastLeaf = factory.Create( TimePeriodEnum.Month3, lastLeaf, _TimeBoundaryCalculateStrategy );
			lastLeaf = factory.Create( TimePeriodEnum.Month6, lastLeaf, _TimeBoundaryCalculateStrategy );
			lastLeaf = factory.Create( TimePeriodEnum.Year, lastLeaf, _TimeBoundaryCalculateStrategy );
			lastLeaf = factory.Create( TimePeriodEnum.Month15, lastLeaf, _TimeBoundaryCalculateStrategy );
			lastLeaf = factory.Create( TimePeriodEnum.Month18, lastLeaf, _TimeBoundaryCalculateStrategy );
			lastLeaf = factory.Create( TimePeriodEnum.Year2, lastLeaf, _TimeBoundaryCalculateStrategy );
			lastLeaf = factory.Create( TimePeriodEnum.Lifetime, lastLeaf, _TimeBoundaryCalculateStrategy );

			return lastLeaf;
		}
	}
}