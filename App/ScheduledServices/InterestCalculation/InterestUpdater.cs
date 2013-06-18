using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using NHibernate;
using NHibernate.Linq;
using Scorto.Service.Scheduler;
using StructureMap;
using log4net;

namespace ScheduledServices.InterestCalculation
{
    public class InterestUpdater : ScheduledExecution
    {
        private static ILog _log = LogManager.GetLogger(typeof(InterestUpdater));

        public override void ExecuteIteration()
        {
            //if (DateTime.UtcNow.Hour != 1) return;
            UpdateLoans();
        }

        public void UpdateLoans()
        {
            _log.Info("Starting updating loan interest");

            var session = ObjectFactory.GetInstance<ISessionFactory>().OpenSession();
            var loans = new LoanRepository(session);
            var updater = new LoanUpdater();

            var loanIds = loans.NotPaid().Select(l => l.Id);

            Loan loan = null;

            foreach (var loanId in loanIds)
            {
                loans.EnsureTransaction(() =>
                    {
                        loan = loans.GetAll()
                                    .Where(l => l.Id == loanId)
                                    .FetchMany(l => l.Transactions)
                                    .First();
                        try
                        {
                            updater.UpdateLoan(loan);
                            loans.SaveOrUpdate(loan);
                            _log.InfoFormat("Loan #{0} updated.", loanId);
                        }
                        catch (Exception e)
                        {
                            _log.ErrorFormat("Failed to update loan #{0}", loanId);
                            _log.Debug(e);
                            session.Evict(loan);
                        }
                    });
                if (loan != null) session.Evict(loan);
            }
        }
    }
}