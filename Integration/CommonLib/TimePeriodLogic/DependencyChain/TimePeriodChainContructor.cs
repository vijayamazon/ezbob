using System;
using System.Collections.Generic;
using System.Linq;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic.DependencyChain.Factories;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain
{
	public static class TimePeriodChainContructor
	{
		public static TimePeriodChain CreateTimePeriodChain( ITimePeriodNodeFactory timePeriodNodeFactory, ITimePeriodNodesCreationTreeFactory timePeriodNodesCreationTreeFactory )
		{
			return new TimePeriodChain( timePeriodNodesCreationTreeFactory.Create( timePeriodNodeFactory ) );
		}


		public static TimePeriodChainWithData<T> CreateDataChain<T>( TimePeriodNodeWithDataFactory<T> timePeriodNodeFactory, ReceivedDataListTimeDependentBase<T> allData, ITimePeriodNodesCreationTreeFactory timePeriodNodesCreationTreeFactory )
			where T : class, ITimeRangedData
		{

			if ( allData == null || allData.Count == 0 )
			{
				return new TimePeriodChainWithNoData<T>();
			}
			TimePeriodNode nodesChain = timePeriodNodesCreationTreeFactory.Create(timePeriodNodeFactory);

			var chain = new TimePeriodChainWithData<T>( nodesChain );

			var leaf = chain.Leaf as TimePeriodNodeWithData<T>;

			do
			{
				leaf.SetSourceData( allData );

				leaf = leaf.Parent as TimePeriodNodeWithData<T>;

			} while ( leaf != null);

			return chain;
		}

		private static ReceivedDataListTimeDependentBase<T> ExtractData<T>( ReceivedDataListTimeDependentBase<T> allData, DateTime fromDate, DateTime toDate )
			where T : class, ITimeRangedData
		{
			IEnumerable<T> rez = allData.Where( d => d.InRange(fromDate, toDate )).Select( d => d );
			return allData.Create( allData.SubmittedDate, rez );
		}

		public static Dictionary<TimePeriodEnum, ReceivedDataListTimeDependentInfo<T>> ExtractDataWithCorrectTimePeriod<T>( TimePeriodChainWithData<T> chain, DateTime updatedDate )
			where T : class, ITimeRangedData
		{

			if ( chain == null || chain.HasNoData )
			{
				return null;
			}

			var dict = new Dictionary<TimePeriodEnum, ReceivedDataListTimeDependentInfo<T>>();
			var leaf = chain.Leaf as TimePeriodNodeWithData<T>;

			var timePeriodChainAll = new TimePeriodChainWithData<T>( leaf.Root );
			var lastNodewithData = timePeriodChainAll.AllNodesWithData.FirstOrDefault( n => timePeriodChainAll.AllNodesWithData.Max( t => t.TimePeriodType ) == n.TimePeriodType );
			TimePeriodChainWithData<T> timePeriodChain = null;

			do
			{
				timePeriodChain = new TimePeriodChainWithData<T>( leaf );
				var timePeriodType = timePeriodChain.TimePeriodType;

				if ( timePeriodType != TimePeriodEnum.Lifetime &&
						!timePeriodChain.HasFullTimePeriodData( updatedDate ) &&
						lastNodewithData == timePeriodChain.Root
					)
				{
					timePeriodType = TimePeriodEnum.Lifetime;
				}

				var allData = timePeriodChain.GetAllData();

				if ( allData != null )
				{
					int countMonths = timePeriodChain.GetCountMonthsForAllData( updatedDate );

					var allDataInfo = new ReceivedDataListTimeDependentInfo<T>( allData, timePeriodType, countMonths );

					dict.Add( timePeriodType, allDataInfo );
				}

				leaf = leaf.Parent as TimePeriodNodeWithData<T>;

			}
			//while ( leaf != null );
			while ( lastNodewithData != timePeriodChain.Root );

			return dict;
		}		
	}

	
}