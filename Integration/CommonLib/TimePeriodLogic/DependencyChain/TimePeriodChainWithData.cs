using System;
using System.Collections.Generic;
using System.Linq;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain
{
	public class TimePeriodChainWithNoData<T> : TimePeriodChainWithData<T>
		where T : class, ITimeRangedData
	{
		public TimePeriodChainWithNoData() 
			: base(null)
		{
		}


	}

	public class TimePeriodChainWithData<T> : TimePeriodChain
		where T : class, ITimeRangedData
	{
		public TimePeriodChainWithData(TimePeriodNode initNode) 
			: base(initNode)
		{

		}

		public IEnumerable<TimePeriodNodeWithData<T>> AllNodesWithData
		{
			get 
			{
				var list = new List<TimePeriodNodeWithData<T>>();

				if ( !HasAtLeastOneNode )
				{
					return list;
				}

				var node = Root as TimePeriodNodeWithData<T>;

				do
				{
					if ( node.HasData )
					{
						list.Add(node);
					}
					node = node.Child as TimePeriodNodeWithData<T>;
				}
				while ( node != null );


				return list;
			}
		}

		public int CountNodesWithData
		{
			get 
			{
				return AllNodesWithData.Count();
			}
		}

		public int CountData
		{
			get 
			{
				var data = GetAllData();
				return !HasAtLeastOneNode || data == null ? 0 : data.Count;
			}
		}

		public bool HasNoData
		{
			get { return !HasData; }
		}

		public bool HasData
		{
			get { return CountNodesWithData > 0; }
		}

		public TimePeriodEnum TimePeriodType
		{
			get { return Root.TimePeriodType; }
		}

		public ReceivedDataListTimeDependentBase<T> GetAllData()
		{
			if ( Root == null )
			{
				return null;
			}

			var root = Root as TimePeriodNodeWithData<T>;

			ReceivedDataListTimeDependentBase<T> data = root.GetThisTimePeriodData();

			if ( data == null )
			{
				return null;
			}
			
			var allData = data.Create( data.SubmittedDate, data );

			var child = root.Child  as TimePeriodNodeWithData<T>;

			while ( child != null )
			{
				data = child.GetThisTimePeriodData( root.TimeBoundaryCalculateStrategy );

				if ( data != null )
				{
					allData.AddRange( data );
				}

				child = child.Child as TimePeriodNodeWithData<T>;
			}

			return allData;
		}

		public int GetCountMonthsForAllData(DateTime updatedDate)
		{
			if ( Root == null )
			{
				return 0;
			}
			
			var root = Root as TimePeriodNodeWithData<T>;

			var data = GetAllData();

			if ( data == null )
			{
				return 0;
			}
			return data.CountMonthFor(updatedDate, root.TimeBoundaryCalculateStrategy) ;
		}

		public bool HasFullTimePeriodData( DateTime date )
		{
			if ( Root == null )
			{
				return false;
			}
			var root = Root as TimePeriodNodeWithData<T>;

			return root.HasFullTimePeriodData( date );

		}
	}
}