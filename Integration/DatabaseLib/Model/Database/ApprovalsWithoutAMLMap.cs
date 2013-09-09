namespace EZBob.DatabaseLib.Model.Database
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class ApprovalsWithoutAMLMap : ClassMap<ApprovalsWithoutAML>
	{
		public ApprovalsWithoutAMLMap()
		{
			Table("ApprovalsWithoutAML");
			Id(x => x.Id);

			Map(x => x.CustomerId);
			Map(x => x.Username).Length(100);
			Map(x => x.Timestamp).CustomType<UtcDateTimeType>();
			Map(x => x.DoNotShowAgain);
		}
	}
}