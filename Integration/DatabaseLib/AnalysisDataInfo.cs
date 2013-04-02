using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper;
using NHibernate.Linq;

namespace EZBob.DatabaseLib
{
	public interface IAnalysisDataInfo
	{
		IDatabaseCustomerMarketPlace DatabaseCustomerMarketPlace { get; }
		Dictionary<DateTime, List<IAnalysisDataParameterInfo>> Data { get; }
	}

	public class AnalysisDataInfo : IAnalysisDataInfo
	{
		public IDatabaseCustomerMarketPlace DatabaseCustomerMarketPlace { get; private set; }

		public Dictionary<DateTime, List<IAnalysisDataParameterInfo>> Data { get; private set; }

		public AnalysisDataInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace )
		{
			Data = new Dictionary<DateTime, List<IAnalysisDataParameterInfo>>();
			DatabaseCustomerMarketPlace = databaseCustomerMarketPlace;
		}

		public AnalysisDataInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IEnumerable<KeyValuePair<DateTime, List<IAnalysisDataParameterInfo>>> data)
			:this(databaseCustomerMarketPlace)
		{
			AddDataRange(data);
		}

		public void AddData( DateTime date, IAnalysisDataParameterInfo data )
		{
			AddData(date, new[] {data});
		}

		public void AddDataRange(IEnumerable<KeyValuePair<DateTime, List<IAnalysisDataParameterInfo>>> data)
		{
			if (data == null || !data.Any())
			{
				return;
			}
			data.ForEach(i => AddData(i.Key, i.Value));
		}

		public void AddData( DateTime date, IEnumerable<IAnalysisDataParameterInfo> data )
		{
			if ( data == null || !data.Any() )
			{
				return;
			}

			List<IAnalysisDataParameterInfo> list;
			if ( !Data.TryGetValue( date, out list ) )
			{
				list = new List<IAnalysisDataParameterInfo>();
				Data.Add( date, list );
			}

			list.AddRange( data );
		}
	}
}