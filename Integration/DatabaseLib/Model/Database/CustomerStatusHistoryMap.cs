namespace EZBob.DatabaseLib.Model.Database
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public sealed class CustomerStatusHistoryMap : ClassMap<CustomerStatusHistory> 
    {
		public CustomerStatusHistoryMap()
        {
			Table("CustomerStatusHistory");
            LazyLoad();
			Id(x => x.Id);
			Map(x => x.Username).Length(100);
			Map(x => x.Timestamp).CustomType<UtcDateTimeType>();
			Map(x => x.CustomerId);
			Map(x => x.PreviousStatus);
			Map(x => x.NewStatus);
        }
    }
}
