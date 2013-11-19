namespace EzBob.CommonLib.TimePeriodLogic
{
	using System;

	public static class AnnualizeHelper
	{
		public static double AnnualizeSum(TimePeriodEnum timePeriodType, DateTime submittedDate, double sum)
		{
			int numOfDays;
			switch (timePeriodType)
			{
				case TimePeriodEnum.Month:
					numOfDays = GetDaysPerMonth(submittedDate.AddMonths(-1));
					break;
				case TimePeriodEnum.Month3:
					numOfDays = GetDaysPerMonth(submittedDate.AddMonths(-2)) + GetDaysPerMonth(submittedDate.AddMonths(-1)) + submittedDate.Day;
					break;
				case TimePeriodEnum.Month6:
					numOfDays = GetDaysPerMonth(submittedDate.AddMonths(-5)) + GetDaysPerMonth(submittedDate.AddMonths(-4)) + GetDaysPerMonth(submittedDate.AddMonths(-3)) + GetDaysPerMonth(submittedDate.AddMonths(-2)) + GetDaysPerMonth(submittedDate.AddMonths(-1)) + submittedDate.Day;
					break;
				case TimePeriodEnum.Year:
					return sum;
				default:
					return 0;
			}

			return sum / numOfDays * 365.0;
		}

		private static int GetDaysPerMonth(DateTime date)
		{
			DateTime dateTime = date.AddMonths(1);
			return new DateTime(dateTime.Year, dateTime.Month, 1).AddDays(-1).Day;
		}
	}
}