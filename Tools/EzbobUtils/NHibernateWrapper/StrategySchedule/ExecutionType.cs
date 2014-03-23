using System;
namespace NHibernateWrapper.StrategySchedule
{
	[System.Flags]
	[System.Serializable]
	public enum ExecutionType
	{
		OneAtATime = 1,
		SingleRun = 2,
		Exclusive = 4
	}
}
