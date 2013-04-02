using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using NHibernate;
using PaymentServices.Calculators;
using log4net;

namespace ezmanage
{
    public class LoanRefresher
    {
        private readonly ILoanRepository _loans;
        private readonly ISession _session;

        public static ILog _log = LogManager.GetLogger("ezmanage.LoanRefresher");

        public LoanRefresher(ILoanRepository loans, ISession session)
        {
            _loans = loans;
            _session = session;
        }

        public void Refresh(DateTime now)
        {
            var loans = _loans.GetAll().ToList();
            foreach (var loan in loans)
            {
                RefreshLoan(now, loan);
            }
        }

        private void RefreshLoan(DateTime now, Loan loan)
        {
            try
            {

                if (loan.Customer == null)
                {
                    _log.Warn("Loan {0} has no customers");
                    return;
                }

                var calculator = new LoanScheduleCalculator();
                calculator.Term = loan.Schedule.Count;
                calculator.Interest = loan.InterestRate;

                calculator.Calculate(loan.LoanAmount, loan, loan.Date);

                using (var tx = _session.BeginTransaction())
                {
                    var calc = new PayEarlyCalculator2(loan, now);
                    var state = calc.GetState();

                    if (loan.Schedule.Any(s => s.Status == LoanScheduleStatus.Late))
                    {
                        _log.InfoFormat("Late payment for loan {0}({3}) of {1} pounds customer id {4} - {2}", loan.Id, state.AmountDue, loan.Customer.Name, loan.Status, loan.Customer.Id);
                    }
                    else if(loan.Schedule.Any(s => s.Date.Date == now.Date && s.Status == LoanScheduleStatus.StillToPay))
                    {
                        _log.InfoFormat("Normal payment for loan {0}({3}) of {1} pounds customer id {4} - {2}", loan.Id, state.AmountDue, loan.Customer.Name, loan.Status, loan.Customer.Id);
                    }

                    _log.Debug(loan.ToString());

                    _loans.Update(loan);
                    tx.Commit();
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Failed to update loan {0}, customer {1}({2})", loan.Id, loan.Customer.Id, loan.Customer.Name);
                _log.Error(e);
            }
        }
    }
}