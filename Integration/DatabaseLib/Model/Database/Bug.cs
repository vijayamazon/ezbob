using System;
using System.Linq;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
    public enum BugState
    {
        Opened, Closed, Reopened
    }

    public class BugStateType : EnumStringType<BugState> { }

    public class Bug
    {
        public virtual int Id { get; set; }
        public virtual BugState State { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual string Type { get; set; }
        public virtual int? MarketPlaceId { get; set; }
        public virtual int? CreditBureauDirectorId { get; set; }
        public virtual DateTime DateOpened { get; set; }
        public virtual DateTime? DateClosed { get; set; }
        public virtual string TextOpened { get; set; }
        public virtual string TextClosed { get; set; }
        public virtual User UnderwriterOpened { get; set; }
        public virtual User UnderwriterClosed { get; set; } 
    }

    public interface IBugRepository : IRepository<Bug>
    {
        Bug Search(int customerid, string bugtype, int? mp, int? director);
    }

    public class BugRepository : NHibernateRepositoryBase<Bug>, IBugRepository
    {
        public BugRepository(ISession session) : base(session)
        {
        }

        public Bug Search(int customerid, string bugtype, int? mp, int? director)
        {
            return GetAll().FirstOrDefault(b => b.Customer.Id == customerid && b.Type == bugtype && b.MarketPlaceId == mp && b.CreditBureauDirectorId == director);
        }
    }

    public class BugMap : ClassMap<Bug>
    {
        public BugMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            References(x => x.Customer, "CustomerId");
            Map(x => x.Type).Length(200);
            Map(x => x.State).CustomType<BugStateType>();
            Map(x => x.MarketPlaceId);
            Map(x => x.CreditBureauDirectorId);
            Map(x => x.DateOpened);
            Map(x => x.DateClosed);
            Map(x => x.TextOpened).Length(2000);
            Map(x => x.TextClosed).Length(2000);
            References(x => x.UnderwriterOpened, "UnderwriterOpenedId");
            References(x => x.UnderwriterClosed, "UnderwriterClosedId");
        }
    }
}