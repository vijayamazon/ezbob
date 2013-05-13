using System;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Loans
{
    public class LoanChangesHistory
    {
        public virtual int Id { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual Database.Loans.Loan Loan { get; set; }
        public virtual string Data { get; set; }
        public virtual User User { get; set; }
    }

    public interface ILoanChangesHistoryRepository : IRepository<LoanChangesHistory>
    {

    }

    public class LoanChangesHistoryRepository : NHibernateRepositoryBase<LoanChangesHistory>, ILoanChangesHistoryRepository
    {
        public LoanChangesHistoryRepository(ISession session) : base(session)
        {
        }
    }

    public class LoanChangeHistoryMap : ClassMap<LoanChangesHistory>
    {
        public LoanChangeHistoryMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            Map(x => x.Date).CustomType<UtcDateTimeType>();
            Map(x => x.Data).CustomType("StringClob");
            References(x => x.User, "UserId");
            References(x => x.Loan, "LoanId");
        }
    }

}