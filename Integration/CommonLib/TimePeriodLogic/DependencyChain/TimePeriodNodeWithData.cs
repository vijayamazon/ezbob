using System;
using System.Collections.Concurrent;
using System.Linq;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain
{
	public class TimePeriodNodeWithData<T> : TimePeriodNode
		where T : class, ITimeRangedData
	{
		private readonly ITimeBoundaryCalculationStrategy _TimeBoundaryCalculateStrategy;
		private readonly ConcurrentDictionary<TimeBoundaryCalculationStrategyType, ReceivedDataListTimeDependentBase<T>> _Cache = new ConcurrentDictionary<TimeBoundaryCalculationStrategyType,ReceivedDataListTimeDependentBase<T>>();
		private ReceivedDataListTimeDependentBase<T> _AllData;

		public TimePeriodNodeWithData( TimePeriodEnum timePeriod, TimePeriodNode child, ITimeBoundaryCalculationStrategy timeBoundaryCalculateStrategy )
			: base( timePeriod, child )
		{
			_TimeBoundaryCalculateStrategy = timeBoundaryCalculateStrategy;
		}

		private DateTime GetLeftBoundary(DateTime fromDate, ITimeBoundaryCalculationStrategy strategy)
		{
			return strategy.GetLeftBoundary( TimePeriodType, fromDate );
		}

		private DateTime GetRightBoundary(DateTime fromDate, ITimeBoundaryCalculationStrategy strategy)
		{
			if ( Child == null )
			{				
				return strategy.GetRightBoundary( TimePeriodType, fromDate );
			}

			return strategy.GetLeftBoundary( Child.TimePeriodType, fromDate ).AddSeconds( -1 );
		}

		internal TimeBoundaryCalculationStrategyType TimeBoundaryCalculateStrategyType
		{
			get { return _TimeBoundaryCalculateStrategy.Type; }
		}

		public ITimeBoundaryCalculationStrategy TimeBoundaryCalculateStrategy
		{
			get { return _TimeBoundaryCalculateStrategy; }
		}

		public int CountData
		{
			get 
			{
				var data = GetThisTimePeriodData();
				return data != null ? data.Count : 0; 
			}
		}

		public bool HasData
		{
			get
			{
				return CountData > 0;
			}
		}

		public bool HasFullTimePeriodData( DateTime date)
		{
			if ( TimeFrame != TimeFrameType.Monthly )
			{
				throw new NotImplementedException();
			}

			if ( !HasData )
			{
				return false;
			}

			var fromDate = GetLeftBoundary( date, _TimeBoundaryCalculateStrategy);
			DateTime toDate = GetToDate(TimePeriodType, date, fromDate);
			
			var data = GetThisTimePeriodData();
			return data.Any(d => d.InRange(fromDate, toDate));
		}

		private DateTime GetToDate(TimePeriodEnum timePeriodType, DateTime finalDate, DateTime fromDate)
		{
			switch (timePeriodType)
			{
				case TimePeriodEnum.Month:
					return finalDate;
				case TimePeriodEnum.Month3:
					return finalDate.AddMonths(-1).AddSeconds(1);
				case TimePeriodEnum.Month6:
					return fromDate.AddMonths(3).AddSeconds(-1);
				case TimePeriodEnum.Year:
					return fromDate.AddMonths(6).AddSeconds(-1);
				case TimePeriodEnum.Month15:
					return fromDate.AddMonths(3).AddSeconds(-1);
				case TimePeriodEnum.Month18:
					return fromDate.AddMonths(3).AddSeconds(-1);
				case TimePeriodEnum.Year2:
					return fromDate.AddMonths(6).AddSeconds(-1);
				case TimePeriodEnum.Lifetime:
					return finalDate;
				default:
					return DateTime.UtcNow;
			}
		}

		public void SetSourceData(ReceivedDataListTimeDependentBase<T> allData)
		{
			_AllData = allData;
			ClearCache();
		}

		private void ClearCache()
		{
			_Cache.Clear();
		}

		internal ReceivedDataListTimeDependentBase<T> GetThisTimePeriodData()
		{
			return GetThisTimePeriodData( TimeBoundaryCalculateStrategy );
		}

		internal ReceivedDataListTimeDependentBase<T> GetThisTimePeriodData( ITimeBoundaryCalculationStrategy strategy )
		{
			ReceivedDataListTimeDependentBase<T> rez;
			if ( !_Cache.TryGetValue( strategy.Type, out rez ) )
			{
				if ( _AllData == null )
				{
					return null;
				}
				var fromDate = GetLeftBoundary( _AllData.SubmittedDate, strategy );
				var toDate = GetRightBoundary( _AllData.SubmittedDate, strategy );

				var data  = _AllData.Where( d => d.InRange( fromDate, toDate ) ).Select( d => d ).ToArray();
				
				rez = _AllData.Create( _AllData.SubmittedDate, data );

				_Cache.TryAdd( strategy.Type, rez );
			}

			return rez;
		}
	}
}