using System;

namespace EzBob.CommonLib.TimePeriodLogic
{
	public class TimePeriodNone : TimePeriodLifeTime
	{
		public TimePeriodNone(TimePeriodEnum periodType, Guid guid) 
			: base(periodType, guid)
		{
			
		}

		public override int DaysInPeriod
		{
			get { return 0; }
		}

	}
}