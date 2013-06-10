namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_FreeAgentUsersMap : ClassMap<MP_FreeAgentUsers>
	{
		public MP_FreeAgentUsersMap()
		{
			Table("MP_FreeAgentUsers");
			Id(x => x.Id);
			References( x => x.Order, "OrderId" );

			Map(x => x.url).Length(250);
			Map(x => x.first_name).Length(250);
			Map(x => x.last_name).Length(250);
			Map(x => x.email).Length(250);
			Map(x => x.role).Length(250);
			Map(x => x.permission_level);
			Map(x => x.opening_mileage);
			Map(x => x.updated_at).CustomType<UtcDateTimeType>();
			Map(x => x.created_at).CustomType<UtcDateTimeType>();
		}
	}
}