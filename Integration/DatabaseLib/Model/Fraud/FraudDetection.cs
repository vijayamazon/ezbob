using System;
using EZBob.DatabaseLib.Model.Database;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudDetection
    {
        public virtual int Id { get; set; }
        public virtual Customer CurrentCustomer { get; set; }
        public virtual Customer InternalCustomer { get; set; }
        public virtual FraudUser ExternalUser { get; set; }
        public virtual string CurrentField { get; set; }
        public virtual string CompareField { get; set; }
        public virtual string Value { get; set; }
        public virtual DateTime? DateOfCheck { get; set; }
    }

    public sealed class FraudDetectionMap : ClassMap<FraudDetection>
    {
        public FraudDetectionMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            References(x => x.CurrentCustomer, "CurrentCustomerId");
            References(x => x.InternalCustomer, "InternalCustomerId");
            References(x => x.ExternalUser, "ExternalUserId");
            Map(x => x.CurrentField).Length(200);
            Map(x => x.CompareField).Length(200);
            Map(x => x.Value).Length(500);
            Map(x => x.DateOfCheck).CustomType<UtcDateTimeType>();
        }
    }
}