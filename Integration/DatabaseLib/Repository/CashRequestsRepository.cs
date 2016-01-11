namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using NHibernate;

	public interface ICashRequestsRepository : IRepository<CashRequest>
    {
    }

    public class CashRequestsRepository : NHibernateRepositoryBase<CashRequest>, ICashRequestsRepository
    {

        public CashRequestsRepository(ISession session)
            : base(session)
        {
        }
    }
}