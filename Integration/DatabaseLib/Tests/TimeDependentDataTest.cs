using System;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.Tests
{
	internal class TimeDependentDataTest : TimeDependentRangedDataBase
	{
		private readonly DateTime _RecordTime;

		public TimeDependentDataTest(DateTime recordTime)
		{
			_RecordTime = recordTime;
		}

		public override DateTime RecordTime
		{
			get { return _RecordTime; }
		}

		public override string ToString()
		{
			return RecordTime.ToString();
		}
	}
}