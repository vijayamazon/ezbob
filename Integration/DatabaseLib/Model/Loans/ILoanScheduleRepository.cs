using System;
using System.Linq;
using ApplicationMng.Repository;

namespace EZBob.DatabaseLib.Model.Database.Loans
{
    public interface ILoanScheduleRepository : IRepository<LoanScheduleItem>
    {
        IQueryable<LoanScheduleItem> GetByDate(DateTime fromDate, DateTime toDate);
    }
}