namespace EzBob.CommonLib.TimePeriodLogic
{
	using System;
	using System.Globalization;

	public interface ITimeDependentData
	{
		DateTime RecordTime { get; }
	}

	public abstract class TimeDependentRangedDataBase : ITimeDependentData
	{
		public abstract DateTime RecordTime { get; }

		public override string ToString()
		{
			return RecordTime.ToString(CultureInfo.InvariantCulture);
		}
	}
}