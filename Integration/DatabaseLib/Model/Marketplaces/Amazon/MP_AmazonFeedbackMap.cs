using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_AmazonFeedbackMap : ClassMap<MP_AmazonFeedback>
	{
		public MP_AmazonFeedbackMap()
		{
			Table( "MP_AmazonFeedback" );
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			Map( x => x.UserRaining );		
			References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );
			
			HasMany( x => x.FeedbackByPeriodItems ).KeyColumn( "AmazonFeedbackId" ).Cascade.All();

			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}