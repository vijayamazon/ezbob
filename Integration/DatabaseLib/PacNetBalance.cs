using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib
{
    public class PacNetBalance
    {
        public int Id { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime Date { get; set; }
    }

    public class PacNetBalanceMap : ClassMap<PacNetBalance>
    {
        public PacNetBalanceMap()
        {
            Not.LazyLoad();
            Table("PacNetBalance");
            Id(x => x.Id);
            Map(x => x.CurrentBalance, "CurrentBalance");
            Map(x => x.Date);
        }
    }

    public interface IPacNetBalanceRepository : IRepository<PacNetBalance>
    {
        decimal GetFunds();
    }

    public class PacNetBalanceRepository : NHibernateRepositoryBase<PacNetBalance>, IPacNetBalanceRepository
    {
        public PacNetBalanceRepository(ISession session) : base(session)
        {
        }

        public decimal GetFunds()
        {
            var funds = _session.Query<PacNetBalance>().OrderByDescending(x => x.Date).FirstOrDefault();
            if (funds == null) return 0;

            var loans = _session.Query<Model.Database.Loans.Loan>().Where(l => l.Date > funds.Date);
            if (!loans.Any()) return funds.CurrentBalance;

            var taken = loans.Sum(l => l.LoanAmount - l.SetupFee);
            return funds.CurrentBalance - taken;
        }

    }

}