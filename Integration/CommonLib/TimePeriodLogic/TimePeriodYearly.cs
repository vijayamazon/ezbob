using System;

namespace EzBob.CommonLib.TimePeriodLogic
{
	public class TimePeriodYearly :TimePeriodBase
	{
		private readonly int _CountYears;

		public TimePeriodYearly(TimePeriodEnum year, Guid guid, int countYears)
			:base(year, guid)
		{
			_CountYears = countYears;
		}

		public override int DaysInPeriod
		{
			get { return 365 * _CountYears; }
		}

		public override int MonthsInPeriod {
			get { return 12 * _CountYears; }
		}
	}
}