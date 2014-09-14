namespace EZBob.DatabaseLib.Model.Database.Loans {
	using ApplicationMng.Repository;
	using NHibernate;
	using System.Linq;

	public class LoanTransactionMethodRepository: NHibernateRepositoryBase<LoanTransactionMethod> {
		public LoanTransactionMethodRepository(ISession session) : base(session) { } // constructor

		public LoanTransactionMethod FindOrDefault(string sName, string sOtherName = null) {
			LoanTransactionMethod oResult = string.IsNullOrWhiteSpace(sName) ? null : GetAll().FirstOrDefault(x => x.Name == sName); 

			if ((oResult == null) && !string.IsNullOrWhiteSpace(sOtherName))
				oResult = GetAll().FirstOrDefault(x => x.Name == sOtherName);

			if (oResult == null) {
				oResult = GetAll().FirstOrDefault(x => x.Name == LoanTransactionMethod.Default) ??
					LoanTransactionMethod.GetDefault();
			} // if

			return oResult;
		} // Find
	} // class LoanTransactionMethodRepository
} // namespace EZBob.DatabaseLib.Model.Database.Loans
