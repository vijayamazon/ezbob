using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Loans;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
    public class PaymentRolloverRepository : NHibernateRepositoryBase<PaymentRollover>
    {
        public PaymentRolloverRepository(ISession session)
            : base(session)
        {
        }

        public PaymentRollover GetById(int id)
        {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<PaymentRollover> GetByLoanId(int loanId)
        {
            return GetAll().Where(x => x.LoanSchedule.Loan.Id == loanId).ToList();
        }

        public PaymentRollover GetByCheduleId(int scheduleId)
        {
            return GetAll().FirstOrDefault(x => x.LoanSchedule.Id == scheduleId);
        }

        public IEnumerable<PaymentRollover> GetRolloversForCustomer(int customerId)
        {
            return GetAll().Where(x => x.LoanSchedule.Loan.Customer.Id == customerId).ToList();
        }
    }
}
