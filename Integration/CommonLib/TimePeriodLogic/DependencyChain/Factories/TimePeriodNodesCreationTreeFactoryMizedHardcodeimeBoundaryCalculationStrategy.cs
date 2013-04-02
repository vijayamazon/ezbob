using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain.Factories
{
	class TimePeriodNodesCreationTreeFactoryHardCodeTimeBoundaryCalculationStrategy : ITimePeriodNodesCreationTreeFactory
	{
		private readonly ITimeBoundaryCalculationStrategy _TimeBoundaryCalculateStrategyByStep;
		private readonly ITimeBoundaryCalculationStrategy _TimeBoundaryCalculateStrategyByEntire;

		public TimePeriodNodesCreationTreeFactoryHardCodeTimeBoundaryCalculationStrategy()
		{
			_TimeBoundaryCalculateStrategyByStep = TimeBoundaryCalculationStrategyFactory.Create( TimeBoundaryCalculationStrategyType.ByStep );
			_TimeBoundaryCalculateStrategyByEntire = TimeBoundaryCalculationStrategyFactory.Create( TimeBoundaryCalculationStrategyType.ByEntire );

		}

		public TimePeriodNodesCreationTreeFactoryType Type
		{
			get { return TimePeriodNodesCreationTreeFactoryType.HardCodeTimeBoundaryCalculationStrategy; }
		}

		public TimePeriodNode Create(ITimePeriodNodeFactory factory)
		{
			var lastLeaf = factory.Create( TimePeriodEnum.Month, _TimeBoundaryCalculateStrategyByStep );
			lastLeaf = factory.Create( TimePeriodEnum.Month3, lastLeaf, _TimeBoundaryCalculateStrategyByEntire );
			lastLeaf = factory.Create( TimePeriodEnum.Month6, lastLeaf, _TimeBoundaryCalculateStrategyByEntire );
			lastLeaf = factory.Create( TimePeriodEnum.Year, lastLeaf, _TimeBoundaryCalculateStrategyByEntire );
			lastLeaf = factory.Create( TimePeriodEnum.Month15, lastLeaf, _TimeBoundaryCalculateStrategyByEntire );
			lastLeaf = factory.Create( TimePeriodEnum.Month18, lastLeaf, _TimeBoundaryCalculateStrategyByEntire );
			lastLeaf = factory.Create( TimePeriodEnum.Year2, lastLeaf, _TimeBoundaryCalculateStrategyByEntire );
			lastLeaf = factory.Create( TimePeriodEnum.Lifetime, lastLeaf, _TimeBoundaryCalculateStrategyByEntire );

			return lastLeaf;
		}
	}
}