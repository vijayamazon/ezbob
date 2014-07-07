namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_AmazonOrderMap : ClassMap<MP_AmazonOrder>
	{
		public MP_AmazonOrderMap()
		{
			Table( "MP_AmazonOrder" );
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );

			HasMany( x => x.OrderItems )
				.KeyColumn( "AmazonOrderId" )
				.Cascade.All();

			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}