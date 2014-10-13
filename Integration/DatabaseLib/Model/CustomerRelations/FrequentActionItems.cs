namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class FrequentActionItems
	{
		public virtual int Id { get; set; }
		public virtual string Item { get; set; }
		public virtual bool IsActive { get; set; }
	}

	public class FrequentActionItemsMap : ClassMap<FrequentActionItems>
	{
		public FrequentActionItemsMap()
		{
			Table("FrequentActionItems");
			Id(x => x.Id);
			Map(x => x.Item).Length(1000);
			Map(x => x.IsActive);
		}
	}

	public interface IFrequentActionItemsRepository : IRepository<FrequentActionItems>
	{
	}

	public class FrequentActionItemsRepository : NHibernateRepositoryBase<FrequentActionItems>, IFrequentActionItemsRepository
	{
		public FrequentActionItemsRepository(ISession session)
			: base(session)
		{
		}
	}
}
