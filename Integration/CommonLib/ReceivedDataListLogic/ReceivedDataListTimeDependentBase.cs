using System;
using System.Collections.Generic;
using System.Linq;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;

namespace EzBob.CommonLib.ReceivedDataListLogic
{
	public abstract class ReceivedDataListTimeDependentBase<T> : ReceivedDataListBase<T>, IReceivedDataListFactory<T>
		where T : class, ITimeRangedData
	{
		protected ReceivedDataListTimeDependentBase(DateTime submittedDate, IEnumerable<T> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public int CountMonthFor( DateTime forDate, ITimeBoundaryCalculationStrategy timeBoundaryCalculateStrategy )
		{
			if ( !HasData )
			{
				return 0;
			}

			DateTime minDate = _Data.Min( d => d.LeftBoundary );
			
			return timeBoundaryCalculateStrategy.GetCountIncludedMonths( minDate, forDate );
		}

		public abstract ReceivedDataListTimeDependentBase<T> Create(DateTime submittedDate, IEnumerable<T> collection);		
	}
}