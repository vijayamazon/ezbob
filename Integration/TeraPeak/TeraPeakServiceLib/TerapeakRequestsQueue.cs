using System;
using System.Collections.Generic;
using System.Linq;
using EzBob.CommonLib;
using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
using EzBob.TeraPeakServiceLib.Requests.SellerResearch;

namespace EzBob.TeraPeakServiceLib
{
	public class TerapeakRequestsQueue
	{
		private readonly LinkedList<SearchQueryDatesRange> _Data = new LinkedList<SearchQueryDatesRange>();

		public static SearchQueryDatesRangeListData CreateQueriesDates(TeraPeakRequestDataInfo requestInfo, DateTime now)
		{
			var data = new SearchQueryDatesRangeListData();

			DateTime startDate = requestInfo.StartDate.GetFirstMonthDate();
			DateTime endDate;

			for ( int i = 0; i < requestInfo.CountSteps; i++ )
			{
				switch ( requestInfo.StepType )
				{
					case TeraPeakRequestStepEnum.ByMonth:

						if ( i != 0 )
						{
							startDate = startDate.GetNextMonthFirstDate();
						}

						endDate = startDate.GetLastMonthDate();

						break;

					default:
						throw new NotSupportedException();
				}

				if ( startDate > endDate )
				{
					throw new InvalidOperationException();
				}

				data.Add(new SearchQueryDatesRange(startDate, endDate, now.GetFirstMonthDate() == startDate && endDate != now ? RangeMarkerType.Partial : RangeMarkerType.Full));
			}

			return data;
		}

		public TerapeakRequestsQueue( IEnumerable<SearchQueryDatesRange> initDataRange )
		{
			initDataRange.OrderByDescending( i => i.StartDate ).ToList().ForEach( i => _Data.AddFirst( i ) );			
		}

		public bool HasItems
		{
			get { return _Data.Count > 0; }
		}

		public void Remove(SearchQueryDatesRange lastDatesRange)
		{
			RemoveAndCorrectQueue(lastDatesRange, null);
		}

		public void RemoveAndCorrectQueue( SearchQueryDatesRange lastDatesRange, ModifiedDateQuery modifiedRange )
		{			
			if ( modifiedRange == null || modifiedRange.Dates == null )
			{
				_Data.Remove( lastDatesRange );
				return;
			}

			LinkedListNode<SearchQueryDatesRange> currentItem = _Data.Find(lastDatesRange);

			if ( currentItem.Previous != null )
			{
				throw new InvalidOperationException();
			}

			CorrectQueue( modifiedRange.Dates,  currentItem);
		}

		private void CorrectQueue( ModifiedDateRange modifiedRange, LinkedListNode<SearchQueryDatesRange> currentItem )
		{
			var currentItemValue = currentItem.Value;

			if ( _Data.Count == 1 )
			{
				_Data.Clear();
				return;
			}

			if ( currentItemValue.EndDate < modifiedRange.StartDate )
			{
				// если EndData текущего запроса < чем начальная дата модифицированного, то корректируем начальный запрос	
				// т.е. запросы должны начинаться позже
				CorrectQueueLeft( modifiedRange, currentItem );
			}
			else if(currentItemValue.StartDate > modifiedRange.EndDate )
			{
				// если EndData текущего запроса > чем начальная дата модифицированного, 
				// то нам говорят, что больше нет данных
				_Data.Clear();
			}
			else if ( currentItemValue.StartDate < modifiedRange.StartDate && currentItemValue.EndDate >= modifiedRange.EndDate )
			{
				// если начальная дата < модифицированной начально, а конечные совпадают =>
				// нам корректируют диапазон пришедших данных
				_Data.Remove( currentItem );
			}
			else if ( currentItemValue.StartDate == modifiedRange.StartDate && currentItemValue.EndDate > modifiedRange.EndDate )
			{
				// если начальные даты совпадают, а конечные нет, то тоже 
				// нам корректируют диапазон пришедших данныхи больше данных нет
				_Data.Clear();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		private void CorrectQueueLeft(ModifiedDateRange modifiedRange, LinkedListNode<SearchQueryDatesRange> currentItem)
		{
			var currentItemValue = currentItem.Value;

			LinkedListNode<SearchQueryDatesRange> nextItem = currentItem.Next;

			if ( currentItemValue.EndDate < modifiedRange.StartDate )
			{
				_Data.Remove( currentItem );

				CorrectQueueLeft( modifiedRange, nextItem );
			}
			else if ( currentItemValue.StartDate <= modifiedRange.StartDate )
			{
				currentItemValue.StartDate = modifiedRange.StartDate.Value;

				if ( currentItemValue.EndDate <= modifiedRange.EndDate )
				{
					currentItemValue.EndDate = modifiedRange.EndDate.Value;
					
					if ( currentItemValue.EndDate < modifiedRange.EndDate )
					{
						_Data.Clear();
						_Data.AddFirst( currentItemValue );
					}
				}
			}
		}

		public SearchQueryDatesRange Peek()
		{
			return _Data.First.Value;
		}

		public List<SearchQueryDatesRange> Items
		{
			get
			{
				return _Data.OrderBy( i => i.StartDate ).ToList();
			}
		}

		public int Count
		{
			get { return _Data.Count; }
		}
	}
}