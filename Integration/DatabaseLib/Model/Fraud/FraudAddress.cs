﻿using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudAddress
    {
        public virtual int Id { get; set; }
        public virtual string Postcode { get; set; }
        public virtual string Line1 { get; set; }
        public virtual string Line2 { get; set; }
        public virtual string Line3 { get; set; }
        public virtual string Town { get; set; }
        public virtual string County { get; set; }
        public virtual FraudUser FraudUser { get; set; }
    }

    public sealed class FraudAddressMap : ClassMap<FraudAddress>
    {
        public FraudAddressMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            Map(x => x.Postcode);
            Map(x => x.Line1).Length(200);
            Map(x => x.Line2).Length(200);
            Map(x => x.Line3).Length(200);
            Map(x => x.Town).Length(200);
            Map(x => x.County).Length(200);
            References(x => x.FraudUser, "FraudUserId");
        }
    }
}
