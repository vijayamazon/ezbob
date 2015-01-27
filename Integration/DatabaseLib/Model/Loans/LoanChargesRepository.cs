using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Loans
{
    public class LoanChargesRepository : NHibernateRepositoryBase<LoanCharge>
    {
        public LoanChargesRepository(ISession session)
            : base(session)
        {
        }

        public void AddCharge(decimal amount, int loanId, int loanChargesTypeId)
        {
            Save(
                new LoanCharge
                    {
                        Amount = amount,
                        ChargesType = Session.Load<ConfigurationVariable>(loanChargesTypeId),
                        Date = DateTime.UtcNow,
                        Loan = Session.Load<Database.Loans.Loan>(loanId)
                    }
                );
        }

        public IQueryable <LoanCharge> GetByLoanId(int loanId)
        {
            return GetAll().Where(x => x.Loan.Id == loanId);
        }
    }
}