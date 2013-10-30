namespace EZBob.DatabaseLib.Model.Database
{
	using FluentNHibernate.Mapping;

	public class UnderwriterRecentCustomersMap : ClassMap<UnderwriterRecentCustomers>
	{
		public UnderwriterRecentCustomersMap()
		{
			Table("UnderwriterRecentCustomers");
			Id(x => x.Id);

			Map(x => x.UserName).Length(100);
			Map(x => x.CustomerId);
		}
	}
}