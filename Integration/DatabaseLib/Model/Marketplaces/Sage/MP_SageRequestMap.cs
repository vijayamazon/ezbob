namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_SageRequestMap : ClassMap<MP_SageRequest>
	{
		public MP_SageRequestMap()
		{
			Table("MP_SageRequest");
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			HasMany(x => x.Invoices).KeyColumn("RequestId").Cascade.All();
			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}