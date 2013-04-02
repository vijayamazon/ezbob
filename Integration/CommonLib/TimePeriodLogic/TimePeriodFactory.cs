using System;

namespace EzBob.CommonLib.TimePeriodLogic
{
	public static class TimePeriodFactory
	{
		public static ITimePeriod Create( TimePeriodEnum type )
		{
			switch (type)
			{
				case TimePeriodEnum.Month:
					return TimePeriodBase.Month;

				case TimePeriodEnum.Month3:
					return TimePeriodBase.Month3;

				case TimePeriodEnum.Month6:
					return TimePeriodBase.Month6;

				case TimePeriodEnum.Year:
					return TimePeriodBase.Year;

				case TimePeriodEnum.Lifetime:
					return TimePeriodBase.LifeTime;

				case TimePeriodEnum.Zero:
					return TimePeriodBase.Zero;

				case TimePeriodEnum.Month15:
					return TimePeriodBase.Month15;

				case TimePeriodEnum.Month18:
					return TimePeriodBase.Month18;

				case TimePeriodEnum.Year2:
					return TimePeriodBase.Year2;

				default:
					throw new NotImplementedException();
			}
		}

		public static ITimePeriod CreateById( Guid id )
		{
			return TimePeriodBase.GetById( id );
		}
	}
}