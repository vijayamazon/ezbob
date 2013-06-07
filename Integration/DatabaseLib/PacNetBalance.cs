using System;
using System.Linq;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib
{
    public class PacNetBalance
    {
        public int Id { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal ReservedAmount { get; set; }
        public decimal Adjusted { get; set; }
        public decimal Loans { get; set; }
        public DateTime? Date { get; set; }
    }

    public class PacNetBalanceMap : ClassMap<PacNetBalance>
    {
        public PacNetBalanceMap()
        {
            Not.LazyLoad();
            Table("vPacnetBalance");
            Id(x => x.Id);
            Map(x => x.CurrentBalance, "PacnetBalance");
            Map(x => x.ReservedAmount);
            Map(x => x.Adjusted);
            Map(x => x.Loans);
            Map(x => x.Date);
        }
    }

    public interface IPacNetBalanceRepository : IRepository<PacNetBalance>
    {
        decimal GetFunds();
        PacNetBalance GetBalance();
    }

    public class PacNetBalanceRepository : NHibernateRepositoryBase<PacNetBalance>, IPacNetBalanceRepository
    {
        public PacNetBalanceRepository(ISession session) : base(session)
        {
        }

        public decimal GetFunds()
        {
            var funds = GetBalance();
            return funds.Adjusted;
        }

        public PacNetBalance GetBalance()
        {
            return _session.Query<PacNetBalance>().First();
        }
    }

}