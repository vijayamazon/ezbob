namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using System;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class FrequentActionItemsForCustomer
	{
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual int ItemId { get; set; }
		public virtual DateTime? MarkedDate { get; set; }
		public virtual DateTime? UnmarkedDate { get; set; }
	}

	public class FrequentActionItemsForCustomerMap : ClassMap<FrequentActionItemsForCustomer>
	{
		public FrequentActionItemsForCustomerMap()
		{
			Table("FrequentActionItemsForCustomer");
			Id(x => x.Id);
			Map(x => x.CustomerId);
			Map(x => x.ItemId);
			Map(x => x.MarkedDate).CustomType<UtcDateTimeType>();
			Map(x => x.UnmarkedDate).CustomType<UtcDateTimeType>();
		}
	}

	public interface IFrequentActionItemsForCustomerRepository : IRepository<FrequentActionItemsForCustomer>
	{
	}

	public class FrequentActionItemsForCustomerRepository : NHibernateRepositoryBase<FrequentActionItemsForCustomer>, IFrequentActionItemsForCustomerRepository
	{
		public FrequentActionItemsForCustomerRepository(ISession session)
			: base(session)
		{
		}
	}
}
