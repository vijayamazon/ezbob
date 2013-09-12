namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using FluentNHibernate.Mapping;

	public class YodleeBanksMap : ClassMap<YodleeBanks>
	{
		public YodleeBanksMap()
		{
			Table("YodleeBanks");
			Id(x => x.Id);
			Map(x => x.Name).Length(300);
			Map(x => x.ContentServiceId);
			Map(x => x.ParentBank).Length(100);
			Map(x => x.Active);
			Map(x => x.Image);
		}
	}
}