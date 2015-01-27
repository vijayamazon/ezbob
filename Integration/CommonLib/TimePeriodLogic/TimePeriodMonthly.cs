using System;

namespace EzBob.CommonLib.TimePeriodLogic
{

	public class TimePeriodMonthly : TimePeriodBase
	{
		private readonly int _CountMonths;

		public TimePeriodMonthly(TimePeriodEnum month, Guid guid, int countMonths)
			:base(month, guid)
		{
			_CountMonths = countMonths;
		}

		public override int DaysInPeriod
		{
			get { return _CountMonths * 30; }
		}

		public override int MonthsInPeriod {
			get { return _CountMonths; }
		}
	}
}