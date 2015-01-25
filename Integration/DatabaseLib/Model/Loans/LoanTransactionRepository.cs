using System;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Loans
{
	public interface ILoanTransactionRepository : IRepository<LoanTransaction> {
		IQueryable<PaypointTransaction> GetByDate(DateTime dateFrom, DateTime dateTo);
	}

    public class LoanTransactionRepository : NHibernateRepositoryBase<LoanTransaction>, ILoanTransactionRepository
    {
        public LoanTransactionRepository(ISession session) : base(session)
        {
        }

        public IQueryable<PaypointTransaction> GetByDate(DateTime dateFrom, DateTime dateTo)
        {
            return GetAll().OfType<PaypointTransaction>().Where(t => t.PostDate >= dateFrom && t.PostDate < dateTo);
        }
    }
}