using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
    public interface ILoanOptionsRepository : IRepository<LoanOptions>
    {
        LoanOptions GetByLoanId(int loanId);        
    }

    public class LoanOptionsRepository : NHibernateRepositoryBase<LoanOptions>, ILoanOptionsRepository
    {
        public LoanOptionsRepository(ISession session) : base(session)
        {
        }

        public LoanOptions GetByLoanId(int loanId)
        {
            return  Session.QueryOver<LoanOptions>().Where(opt=>opt.LoanId == loanId).SingleOrDefault<LoanOptions>();
        }
    }
}