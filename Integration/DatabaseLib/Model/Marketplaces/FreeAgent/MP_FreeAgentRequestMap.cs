namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_FreeAgentRequestMap : ClassMap<MP_FreeAgentRequest>
	{
		public MP_FreeAgentRequestMap()
		{
			Table("MP_FreeAgentRequest");
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			HasMany(x => x.Invoices).KeyColumn("RequestId").Cascade.All();
			HasMany(x => x.Expenses).KeyColumn("RequestId").Cascade.All();
			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}