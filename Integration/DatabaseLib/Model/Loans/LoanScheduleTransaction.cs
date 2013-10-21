using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database.Loans {
	public class LoanScheduleTransaction {
		public virtual int Id { get; set; }
		public virtual Loan Loan { get; set; }
		public virtual LoanScheduleItem Schedule { get; set; }
		public virtual LoanTransaction Transaction { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual decimal PrincipalDelta { get; set; }
		public virtual decimal InterestDelta { get; set; }
		public virtual decimal FeesDelta { get; set; }
		public virtual LoanScheduleStatus StatusBefore { get; set; }
		public virtual LoanScheduleStatus StatusAfter { get; set; }
	} // class LoanScheduleTransaction {
} // namespace EZBob.DatabaseLib.Model.Database.Loans

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	public class LoanScheduleTransactionMap : ClassMap<Database.Loans.LoanScheduleTransaction>
	{
		public LoanScheduleTransactionMap()
		{
			Id(x => x.Id).GeneratedBy.Native();
			Cache.ReadWrite().Region("LongTerm").ReadWrite();
			References(x => x.Loan, "LoanID");
			References(x => x.Schedule, "ScheduleID");
			References(x => x.Transaction, "TransactionID");
			Map(x => x.PrincipalDelta);
			Map(x => x.InterestDelta);
			Map(x => x.FeesDelta);
			Map(x => x.StatusBefore).CustomType<LoanScheduleStatusType>();
			Map(x => x.StatusAfter).CustomType<LoanScheduleStatusType>();
		} // constructor
	} // class LoanScheduleTransactionMap
} // namespace EZBob.DatabaseLib.Model.Database.Mapping

namespace EZBob.DatabaseLib.Model.Database.Repositories
{
	using ApplicationMng.Repository;
	using NHibernate;

	public interface ILoanScheduleTransactionRepository : IRepository<LoanScheduleTransaction>
	{
	}

	public class LoanScheduleTransactionRepository : NHibernateRepositoryBase<LoanScheduleTransaction>, ILoanScheduleTransactionRepository
	{

		public LoanScheduleTransactionRepository(ISession session)
			: base(session)
		{
		}
	}
} // namespace EZBob.DatabaseLib.Model.Database.Repositories
