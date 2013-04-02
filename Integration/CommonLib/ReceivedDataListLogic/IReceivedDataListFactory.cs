using System;
using System.Collections.Generic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EzBob.CommonLib.ReceivedDataListLogic
{
	public interface IReceivedDataListFactory<T>
		where T : class, ITimeRangedData
	{
		ReceivedDataListTimeDependentBase<T> Create(DateTime submittedDate, IEnumerable<T> collection);
	}
}