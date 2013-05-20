namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using FluentNHibernate.Mapping;

	public class YodleeBanksMap : ClassMap<YodleeBanks>
    {
		public YodleeBanksMap()
        {
			Table("YodleeBanks");
            Id(x => x.Id);
			Map(x => x.Name, "Name").Length(300);
			Map(x => x.ContentServiceId);
        }
    }
}