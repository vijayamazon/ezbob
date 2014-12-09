using System.Diagnostics;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoEbayFeedBack : ResultInfoByServerResponseBase
	{
		private readonly GetFeedbackResponseType  _response;

        public ResultInfoEbayFeedBack(GetFeedbackResponseType response)
			: base( response )
		{
			_response = response;

		}

        public int UniqueNeutralFeedbackCount
        {
            get { return _response.FeedbackSummary == null? 0:  _response.FeedbackSummary.UniqueNeutralFeedbackCount; }
        }

        public int UniquePositiveFeedbackCount
        {
			get { return _response.FeedbackSummary == null ? 0 : _response.FeedbackSummary.UniquePositiveFeedbackCount; }
        }

	    public int UniqueNegativeFeedbackCount
	    {
			get { return _response.FeedbackSummary == null ? 0 : _response.FeedbackSummary.UniqueNegativeFeedbackCount; }
	    }

	    public double RepeatBuyerPercent
	    {
			get { return _response.FeedbackSummary == null || _response.FeedbackSummary.SellerRoleMetrics == null ? 0 : _response.FeedbackSummary.SellerRoleMetrics.RepeatBuyerPercent; }
	    }

        public int RepeatBuyerCount
        {
			get { return _response.FeedbackSummary == null || _response.FeedbackSummary.SellerRoleMetrics == null ? 0 : _response.FeedbackSummary.SellerRoleMetrics.RepeatBuyerCount; }
        }

        public double TransactionPercent
        {
			get { return _response.FeedbackSummary == null || _response.FeedbackSummary.SellerRoleMetrics == null ? 0 : _response.FeedbackSummary.SellerRoleMetrics.TransactionPercent; }
        }

        public int UniqueBuyerCount
        {
			get { return _response.FeedbackSummary == null || _response.FeedbackSummary.SellerRoleMetrics == null ? 0 : _response.FeedbackSummary.SellerRoleMetrics.UniqueBuyerCount; }
        }

	    public override DataInfoTypeEnum DataInfoType
	    {
            get { return DataInfoTypeEnum.Feedback; }
	    }

		public int GetNegativeFeedbackByPeriod(TimePeriodEnum period)
		{
			return _response.FeedbackSummary == null ? 0 : GetFeedbackByPeriod( _response.FeedbackSummary.NegativeFeedbackPeriodArray, period );
		}

		public int GetPositiveFeedbackByPeriod(TimePeriodEnum period)
		{
			return _response.FeedbackSummary == null ? 0 : GetFeedbackByPeriod( _response.FeedbackSummary.PositiveFeedbackPeriodArray, period );
		}

		public int GetNeutralFeedbackByPeriod( TimePeriodEnum period )
		{
			return _response.FeedbackSummary == null ? 0 : GetFeedbackByPeriod( _response.FeedbackSummary.NeutralFeedbackPeriodArray, period );
		}

		private int GetFeedbackByPeriod( FeedbackPeriodType[] feedbackPeriodType, TimePeriodEnum period )
		{
			if (feedbackPeriodType == null || feedbackPeriodType.Length == 0)
			{
				return 0;
			}

			var tp = TimePeriodFactory.Create(period);

			int days = tp.DaysInPeriod;

			FeedbackPeriodType rez = feedbackPeriodType.FirstOrDefault(p => p.PeriodInDays == days);

			Debug.Assert(rez != null);

			if (rez == null)
			{
				return 0;
			}

			return rez.Count;
		}

		public EbayRaitingInfo GetRaitingData( FeedbackSummaryPeriodCodeType feedbackSummaryPeriodCodeType, FeedbackRatingDetailCodeType feedbackRatingDetailCodeType )
		{
			if ( _response.FeedbackSummary == null || _response.FeedbackSummary.SellerRatingSummaryArray == null || _response.FeedbackSummary.SellerRatingSummaryArray.Length == 0 )
			{
				return null;
			}

			AverageRatingSummaryType val = _response.FeedbackSummary.SellerRatingSummaryArray.FirstOrDefault( r => r.FeedbackSummaryPeriod == feedbackSummaryPeriodCodeType );

			if ( val == null )
			{
				return null;
			}

			if( val.AverageRatingDetails == null || val.AverageRatingDetails.Length == 0 || !val.FeedbackSummaryPeriodSpecified)
			{
				return null;
			}

			AverageRatingDetailsType rez = val.AverageRatingDetails.FirstOrDefault( a => a.RatingDetail == feedbackRatingDetailCodeType );

			if ( rez == null )
			{
				return null;
			}

			return new EbayRaitingInfo
				{
					Value = rez.Rating,
					Count = rez.RatingCount
				};
		}

	}

}

