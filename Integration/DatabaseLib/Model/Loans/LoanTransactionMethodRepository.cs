using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Loans {
	public class LoanTransactionMethodRepository: NHibernateRepositoryBase<LoanTransactionMethod> {
		public LoanTransactionMethodRepository(ISession session) : base(session) { } // constructor
	} // class LoanTransactionMethodRepository
} // namespace EZBob.DatabaseLib.Model.Database.Loans
