namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class MP_YodleeSearchWords
	{
		public virtual int Id { get; set; }
		public virtual string SearchWords { get; set; }
	}

	public class MP_YodleeSearchWordsMap : ClassMap<MP_YodleeSearchWords>
	{
		public MP_YodleeSearchWordsMap()
		{
			Table("MP_YodleeSearchWords");
			Id(x => x.Id);
			Map(x => x.SearchWords, "SearchWords").Length(300);
		}
	}

	public interface IYodleeSearchWordsRepository : IRepository<MP_YodleeSearchWords>
	{
		
	}

	public class YodleeSearchWordsRepository : NHibernateRepositoryBase<MP_YodleeSearchWords>, IYodleeSearchWordsRepository
	{
		public YodleeSearchWordsRepository(ISession session)
			: base(session)
		{
		}
	}
}