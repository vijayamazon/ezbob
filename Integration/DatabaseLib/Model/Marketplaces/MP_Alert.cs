using System;
using ApplicationMng.Model;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public enum AlertsSeverity
    {
        Warning = 0,
        Error = 1,
        Reject=2,
        Passed=3
    }

    public class MP_Alert
    {
        public virtual int Id { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual string AlertType { get; set; }
        public virtual AlertsSeverity AlertSeverity { get; set; }
        public virtual string AlertText { get; set; }
        public virtual string ActionToMake { get; set; }
        public virtual string Status { get; set; }
        public virtual DateTime? ActionDate { get; set; }
        public virtual User Employee { get; set; }
        public virtual int? DirectorId { get; set; }
        public virtual string Details { get; set; }
        public virtual DateTime? StrategyStartedDate { get; set; }
    }

    public sealed class MP_AlertMap : ClassMap<MP_Alert>
    {
        public MP_AlertMap()
        {
            Table("MP_Alert");
            Id( x => x.Id );
            References(x => x.Employee, "UserId");
            References(x => x.Customer, "CustomerId");
            Map(x => x.AlertType);
            Map(x => x.AlertText).CustomType("StringClob");
            Map(x => x.Status);
            Map(x => x.ActionToMake).CustomType("StringClob");
            Map(x => x.ActionDate);
            Map(x => x.AlertSeverity).CustomType<AlertsSeverity>();
            Map(x => x.DirectorId);
            Map(x => x.Details).CustomType("StringClob");
            Map(x => x.StrategyStartedDate);
        }
    }
}

