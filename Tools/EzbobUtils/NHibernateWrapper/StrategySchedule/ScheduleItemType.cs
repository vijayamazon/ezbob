using System;
namespace NHibernateWrapper.StrategySchedule
{
	[System.Serializable]
	public enum ScheduleItemType
	{
		RunOnce,
		Weekly,
		Monthly,
		Hourly,
		Periodically
	}
}
