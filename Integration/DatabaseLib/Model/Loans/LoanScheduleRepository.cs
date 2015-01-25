using System;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Loans
{
	public interface ILoanScheduleRepository : IRepository<LoanScheduleItem> {
		IQueryable<LoanScheduleItem> GetByDate(DateTime fromDate, DateTime toDate);
	}
    public class LoanScheduleRepository : NHibernateRepositoryBase<LoanScheduleItem>, ILoanScheduleRepository
    {
        public LoanScheduleRepository(ISession session)
            : base(session)
        {
        }

        public IQueryable<LoanScheduleItem> GetByDate(DateTime fromDate, DateTime toDate)
        {
            return GetAll().Where(i => i.Date >= fromDate && i.Date < toDate);
        }

        public LoanScheduleItem GetById(int scheduleId)
        {
            return GetAll().FirstOrDefault(x => x.Id == scheduleId);
        }

        public LoanScheduleItem GetNextScheduleById(int scheduleId)
        {
            var currentSchedule = GetById(scheduleId);
            
            return 
                currentSchedule == null ? 
                null : 
                GetAll().FirstOrDefault(x => x.Loan.Id == currentSchedule.Loan.Id && x.Id > scheduleId);
        }
    }
}