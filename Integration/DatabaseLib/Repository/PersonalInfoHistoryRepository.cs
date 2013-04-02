using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
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
            return _session.Get<PersonalInfoHistory>(id);
        }

 
    }

}
