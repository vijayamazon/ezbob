namespace EzBob.TeraPeakServiceLib
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using CommonLib;
	using CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
	using CommonLib.TrapForThrottlingLogic;
	using Requests.SellerResearch;

	public class TeraPeakRequestInfo
	{
		public TeraPeakSellerInfo SellerInfo { get; private set; }
		public SearchQueryDatesRangeListData Ranges { get; private set; }

	    public ErrorRetryingInfo ErrorRetryingInfo { get; private set; }

        public TeraPeakRequestInfo(TeraPeakSellerInfo info, SearchQueryDatesRange range, ErrorRetryingInfo errorRetryingInfo)
			: this( info, new SearchQueryDatesRangeListData( range ), errorRetryingInfo )
		{
		}

        public TeraPeakRequestInfo(TeraPeakSellerInfo info, SearchQueryDatesRangeListData ranges, ErrorRetryingInfo errorRetryingInfo)
		{
			SellerInfo = info;
			Ranges = ranges;
            ErrorRetryingInfo = errorRetryingInfo;
		}
	}

	public class TeraPeakSearchBySellerRequester : TeraPeakRequester
	{
		private readonly static ITrapForThrottling _SearchBySellerTrapForThrottling;

		static TeraPeakSearchBySellerRequester()
		{
			_SearchBySellerTrapForThrottling = TrapForThrottlingController.CreateSimpleWait( "Search by Seller", 6, RequestQuoteTimePeriodType.PerSecond );
		}

		public TeraPeakSearchBySellerRequester(ITeraPeakService service)
			:base(service)
		{			
		}

		public TeraPeakDatabaseSellerData Run( TeraPeakRequestInfo requestInfo )
		{
			Contract.Ensures( Contract.Result<String>() != null );
			var now = DateTime.UtcNow;
			var ranges = requestInfo.Ranges;
			var info = requestInfo.SellerInfo;

			var resultSellerInfo = new ResultSellerInfo
			{				
				ReturnDurationData = true,
                ReturnCategoryInformation = true,
                ReturnAllData = true
			};

			var queue = new TerapeakRequestsQueue( ranges );

			var data = new TeraPeakDatabaseSellerData( now );

			string sellerId = info.Id;

			while ( queue.HasItems )
			{
				SearchQueryDatesRange datesRange = queue.Peek();

				var actionName = "SearchBySeller";
			    var errorRetryingInfo = requestInfo.ErrorRetryingInfo;
                var retryingController = new WaitBeforeRetryController(new ErrorRetryingWaiter(), errorRetryingInfo);

                var rez = retryingController.DoForAllTypesOfErrors( () =>
			        _SearchBySellerTrapForThrottling.Execute(() =>
			                                                    {
			                                                        var result = Service.SearchBySeller(sellerId, resultSellerInfo, datesRange);
			                                                        data.IncrementRequests(actionName, datesRange.ToString());
			                                                        return result;
			                                                    },
			                                                 actionName)
                                                             );

				var isErrorNoData = rez.HasError && rez.Errors.Any( e => e.Error.Id == 1003 );

				if ( rez.HasError && !isErrorNoData )
				{
					var error = ExtractError( rez );
					data.Error = error;
					WriteError( info, datesRange, error );
					break;
				}

				data.Add(ParseResultData(datesRange, rez));

				queue.Remove(datesRange);

			}

			return data;
		}

		private string ExtractError( GetSellerResearchResults data )
		{
			if ( data == null || !data.HasError )
			{
				return string.Empty;
			}

			return string.Join( "\n\t", data.Errors.Select( ( e, i ) => string.Format( "#{0} id:{1} - {2}", i + 1, e.Error.Id, e.Error.Error ) ) );
		}

		private void WriteError(TeraPeakSellerInfo sellerInfo, SearchQueryDatesRange queryDates, string error)
		{
			WriteToLog( string.Format( "Terapeak SearchBySellerRequester [{1} {2}] user: {0}, errors: \n{3} ", sellerInfo.Id, queryDates.StartDate, queryDates.EndDate, error ), WriteLogType.Error );
		}

		private bool HasDataInResult( GetSellerResearchResults data )
		{
			return data != null && data.SearchResults != null && data.SearchResults.Statistics != null;
		}

		private TeraPeakDatabaseSellerDataItem ParseResultData( SearchQueryDatesRange searchQueryDates, GetSellerResearchResults data )
		{
			if ( !HasDataInResult(data) )
			{
				return null;
			}

			var startDate = searchQueryDates.StartDate;
			var endDate = searchQueryDates.EndDate;
			return CreateTeraPeakDatabaseSellerDataItem(searchQueryDates, startDate, endDate, data);
		}

	    private static TeraPeakDatabaseSellerDataItem CreateTeraPeakDatabaseSellerDataItem(SearchQueryDatesRange searchQueryDates, DateTime startDate, DateTime endDate, GetSellerResearchResults data)
	    {
            var stat = data.SearchResults.Statistics;

	        return new TeraPeakDatabaseSellerDataItem( startDate, endDate )
	            {
	                AverageSellersPerDay = stat.AverageSellersPerDay,
	                Bids = stat.Bids,
	                ItemsOffered = stat.ItemsOffered,
	                ItemsSold = stat.ItemsSold,
	                Listings = stat.Listings,
	                Revenue = stat.Revenue,
	                SuccessRate = stat.SuccessRate,
	                Successful = stat.Successful,
	                Transactions = stat.Transactions,
	                RangeMarker = searchQueryDates.RangeMarker,
                    Categories = CreateCategories(data)
	            };
	    }

	    private static List<CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData.CategoryStatistics> CreateCategories(GetSellerResearchResults data)
	    {
		    if (data.SearchResults.Categories == null)
		    {
			    return new List<CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData.CategoryStatistics>();
		    }

		    return data.SearchResults.Categories.Select(CreateCategory).ToList();
	    }

        private static CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData.CategoryStatistics CreateCategory(Category category)
        {
            var stat = category.Statistics;

            var tpCategory = new TeraPeakCategory
                {
                    FullName = category.FullName,
                    Id = category.Id,
                    Level = category.Level,
                    Name = category.Name,
                    ParentCategoryID = category.ParentCategoryID
                };

            return new CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData.CategoryStatistics
	            {
	                ItemsSold = stat.ItemsSold,
                    Listings = stat.Listings,
                    Revenue = stat.Revenue,
                    SuccessRate = stat.SuccessRate,
                    Successful = stat.Successful,
                    Category = tpCategory
	            };
        }

	    private void WriteToLog( string message, WriteLogType messageType = WriteLogType.Debug, Exception ex = null )
		{
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine( message );
		}
	}
}
