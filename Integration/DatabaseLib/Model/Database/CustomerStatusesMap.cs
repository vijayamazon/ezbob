namespace EZBob.DatabaseLib.Model.Database
{
	using FluentNHibernate.Mapping;

    public sealed class CustomerStatusesMap : ClassMap<CustomerStatuses> 
    {
		public CustomerStatusesMap()
        {
			Table("CustomerStatuses");
            LazyLoad();
			Id(x => x.Id);
			Map(x => x.Name);
			Map(x => x.IsEnabled);
        }
    }
}
