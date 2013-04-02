using System;
using System.Linq;
using ApplicationMng.Repository;

namespace EZBob.DatabaseLib.Model.Database.Loans
{
    public interface ILoanTransactionRepository : IRepository<LoanTransaction>
    {
        IQueryable<PaypointTransaction> GetByDate(DateTime dateFrom, DateTime dateTo);
    }
}