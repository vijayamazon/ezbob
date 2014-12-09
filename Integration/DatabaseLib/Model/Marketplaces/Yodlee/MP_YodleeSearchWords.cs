namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using System.Linq;
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

		public bool IsExists(string word)
		{
			return _session.QueryOver<MP_YodleeSearchWords>().Where(c => c.SearchWords == word).SingleOrDefault<MP_YodleeSearchWords>() != null;
		}

		public void AddWord(string word)
		{
			if (IsExists(word))
			{
				return;
			}
			var searchWords = new MP_YodleeSearchWords { SearchWords = word };
			Save(searchWords);
		}

		public void DeleteWord(string word)
		{
			if (!IsExists(word))
			{
				return;
			}

			var property = _session.QueryOver<MP_YodleeSearchWords>().Where(c => c.SearchWords == word).SingleOrDefault<MP_YodleeSearchWords>();
			Delete(property);
		}
	}
}
