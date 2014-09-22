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
        public virtual string Concurrence { get; set; }
		public virtual FraudRequest FraudRequest { get; set; }
    }

    public sealed class FraudDetectionMap : ClassMap<FraudDetection>
    {
        public FraudDetectionMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("10000");
            References(x => x.CurrentCustomer, "CurrentCustomerId");
            References(x => x.InternalCustomer, "InternalCustomerId");
            References(x => x.ExternalUser, "ExternalUserId");
            Map(x => x.CurrentField).Length(200);
            Map(x => x.CompareField).Length(200);
            Map(x => x.Value).Length(500);
            Map(x => x.Concurrence).Length(500);
	        References(x => x.FraudRequest, "FraudRequestId");
        }
    }
}