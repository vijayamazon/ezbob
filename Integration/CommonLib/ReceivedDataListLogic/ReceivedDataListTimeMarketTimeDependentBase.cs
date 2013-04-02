using System;
using System.Collections.Generic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EzBob.CommonLib.ReceivedDataListLogic
{
	public abstract class ReceivedDataListTimeMarketTimeDependentBase<T> : ReceivedDataListTimeDependentBase<T>
		where T : class, ITimeRangedData, ITimeDependentData
	{
		protected ReceivedDataListTimeMarketTimeDependentBase(DateTime submittedDate, IEnumerable<T> collection = null) 
			: base(submittedDate, collection)
		{
		}
	}
}