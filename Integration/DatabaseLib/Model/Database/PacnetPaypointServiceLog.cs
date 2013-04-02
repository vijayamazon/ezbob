using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class PacnetPaypointServiceLog
    {
        public virtual long Id { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual DateTime? InsertDate { get; set; }
        public virtual string RequestType { get; set; }
        public virtual string Status { get; set; }
        public virtual string ErrorMessage { get; set; }
    }

    public class PacnetPaypointServiceLogMap : ClassMap<PacnetPaypointServiceLog>
    {
        public PacnetPaypointServiceLogMap()
        {
            Table("PacnetPaypointServiceLog");
            Id(x => x.Id);
            References(x => x.Customer, "CustomerId");
            Map(x => x.InsertDate);
            Map(x => x.RequestType).CustomType("StringClob").LazyLoad();
            Map(x => x.Status).Length(50);
            Map(x => x.ErrorMessage).CustomType("StringClob");
        }
    }
}
