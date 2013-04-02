using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using EzBob.CommonLib;
using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
using EzBob.CommonLib.TrapForThrottlingLogic;
using EzBob.TeraPeakServiceLib.Requests.SellerResearch;
using log4net;

namespace EzBob.TeraPeakServiceLib
{
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
		private static readonly ILog _Log = LogManager.GetLogger( typeof( TeraPeakSearchBySellerRequester ) );
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
				ReturnDurationData = true
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
				
				data.Add(ParceResultData(datesRange, rez));

				
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

		private TeraPeakDatabaseSellerDataItem ParceResultData( SearchQueryDatesRange searchQueryDates, GetSellerResearchResults data )
		{
			if ( !HasDataInResult(data) )
			{
				return null;
			}
			var stat = data.SearchResults.Statistics;

			var startDate = searchQueryDates.StartDate;
			var endDate = searchQueryDates.EndDate;
			return new TeraPeakDatabaseSellerDataItem( startDate, endDate )
			       	{
			       		AverageSellersPerDay = ParveValueInt( stat.AverageSellersPerDay ),
			       		Bids = ParveValueInt( stat.Bids ),
			       		ItemsOffered = ParveValueInt( stat.ItemsOffered ),
			       		ItemsSold = ParveValueInt( stat.ItemsSold ),
			       		Listings = ParveValueInt( stat.Listings ),
			       		Revenue = ParveValueDouble( stat.Revenue ),
			       		SuccessRate = ParveValueDouble( stat.SuccessRate ),
			       		Successful = ParveValueInt( stat.Successful ),
			       		Transactions = ParveValueInt( stat.Transactions ),
						RangeMarker = searchQueryDates.RangeMarker
			       	};
		}

		private int? ParveValueInt( string value )
		{
			return String.IsNullOrWhiteSpace( value ) ? (int?)null : Int32.Parse( value );
		}

		private double? ParveValueDouble( string value )
		{
			return String.IsNullOrWhiteSpace( value ) ? (double?)null : Double.Parse( value );
		}

		private void WriteToLog( string message, WriteLogType messageType = WriteLogType.Debug, Exception ex = null )
		{
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine( message );
		}
	}
}