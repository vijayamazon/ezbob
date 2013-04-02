using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib
{
    public class AvailableFunds
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
    }

    public class AvailableFundsMap : ClassMap<AvailableFunds>
    {
        public AvailableFundsMap()
        {
            Not.LazyLoad();
            Table("AvailableFunds");
            Id(x => x.Id);
            Map(x => x.Value);
            Map(x => x.Date);
        }
    }

    public interface IAvailableFundsRepository : IRepository<AvailableFunds>
    {
        decimal GetFunds();
    }

    public class AvailableFundsRepository : NHibernateRepositoryBase<AvailableFunds>, IAvailableFundsRepository
    {
        public AvailableFundsRepository(ISession session) : base(session)
        {
        }

        public decimal GetFunds()
        {
            var funds = _session.Query<AvailableFunds>().OrderByDescending(x => x.Date).FirstOrDefault();
            if (funds == null) return 0;
            
            var loans = _session.Query<Loan>().Where(l => l.Date > funds.Date);
            if (!loans.Any()) return funds.Value;

            var taken = loans.Sum(l => l.LoanAmount - l.SetupFee);
            return funds.Value - taken;
        }

    }

}