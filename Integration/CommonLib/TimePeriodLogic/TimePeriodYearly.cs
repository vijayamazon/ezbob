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

		public override DateTime GetLeftStep(DateTime toDate)
		{
			return toDate.AddYears( -_CountYears );
		}

		public override DateTime GetLeftBoundary(DateTime toDate)
		{
			var b = toDate.AddMonths( -_CountYears*12 + 1 );

			return new DateTime( b.Year, b.Month, 1, 0, 0, 0 );
		}

		public override DateTime GetRightBoundary(DateTime toDate)
		{
			return new DateTime( toDate.Year, 12, 31 );
		}
	}
}