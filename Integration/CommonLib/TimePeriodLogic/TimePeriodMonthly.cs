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

		public override DateTime GetLeftStep(DateTime toDate)
		{
			return toDate.AddMonths( -_CountMonths );
		}

		public override DateTime GetLeftBoundary(DateTime toDate)
		{
			var b = toDate.AddMonths( -_CountMonths + 1);

			return new DateTime( b.Year, b.Month, 1, 0, 0, 0 );
		}

		public override DateTime GetRightBoundary(DateTime toDate)
		{
			return toDate.GetLastMonthDate();			
		}
	}
}