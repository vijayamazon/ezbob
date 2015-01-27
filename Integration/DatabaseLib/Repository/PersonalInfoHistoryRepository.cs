namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using NHibernate;

    public interface IPersonalInfoHistoryRepository:IRepository<PersonalInfoHistory >
    {
        PersonalInfoHistory Get(int id);
    }

    public class PersonalInfoHistoryRepository : NHibernateRepositoryBase<PersonalInfoHistory>, IPersonalInfoHistoryRepository
    {
        public PersonalInfoHistoryRepository(ISession session) 
			: base(session)
		{
		}

        public PersonalInfoHistory Get(int id)
        {
            return Session.Get<PersonalInfoHistory>(id);
        }
    }
}
