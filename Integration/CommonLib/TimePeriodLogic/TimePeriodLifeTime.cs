using System;

namespace EzBob.CommonLib.TimePeriodLogic
{

	public class TimePeriodLifeTime : TimePeriodBase
	{
		private readonly DateTime _LeftBoundary = new DateTime( 1800, 1, 1 );
		private readonly DateTime _RightBoundary = new DateTime( DateTime.Now.Year + 1 , 1, 1 );
		public TimePeriodLifeTime(TimePeriodEnum lifetime, Guid guid)
			:base(lifetime, guid)
		{
			
		}

		public override int DaysInPeriod
		{
			get { return (int) (new DateTime(12, 12, 2012) - _LeftBoundary).TotalDays; }
		}
	}
}