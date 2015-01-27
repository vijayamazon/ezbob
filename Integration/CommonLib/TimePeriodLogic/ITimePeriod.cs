using System;

namespace EzBob.CommonLib.TimePeriodLogic
{
	public interface ITimePeriod
	{
		TimePeriodEnum TimePeriodType { get; }
		Guid InternalId { get; }
		string Description { get; }
		string Name { get; }
		string DisplayName { get; }
		int DaysInPeriod { get; }
		int MonthsInPeriod { get; }
	}
}