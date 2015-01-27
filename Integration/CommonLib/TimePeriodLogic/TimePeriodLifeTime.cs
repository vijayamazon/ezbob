namespace EzBob.CommonLib.TimePeriodLogic
{
	using System;

	public class TimePeriodLifeTime : TimePeriodBase
	{
		private readonly DateTime _LeftBoundary = new DateTime( 1800, 1, 1 );
		public TimePeriodLifeTime(TimePeriodEnum lifetime, Guid guid)
			:base(lifetime, guid)
		{
		}

		public override int DaysInPeriod
		{
			get { return (int) (new DateTime(12, 12, 2012) - _LeftBoundary).TotalDays; }
		}

		public override int MonthsInPeriod {
			get { return 1000; }
		}
	}
}