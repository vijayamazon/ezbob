using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayFeedbackMap : ClassMap<MP_EbayFeedback>
	{
		public MP_EbayFeedbackMap()
		{
			Table( "MP_EbayFeedback" );
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );
			HasMany( x => x.FeedbackByPeriodItems ).KeyColumn( "EbayFeedbackId" ).Cascade.All();
			HasMany( x => x.RaitingByPeriodItems ).KeyColumn( "EbayFeedbackId" ).Cascade.All();
			Map( x => x.RepeatBuyerCount );
			Map( x => x.RepeatBuyerPercent );
			Map( x => x.TransactionPercent );
			Map( x => x.UniqueBuyerCount );
			Map( x => x.UniqueNegativeCount );
			Map( x => x.UniquePositiveCount );
			Map( x => x.UniqueNeutralCount );
			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}