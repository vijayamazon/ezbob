namespace EZBob.DatabaseLib.Model.Fraud {
    using System;
    using FluentNHibernate.Mapping;
    using System.ComponentModel;
    using ApplicationMng.Repository;
    using NHibernate;
    using NHibernate.Type;

    public enum IovationResult {
        [Description("deny request")]   D,
        [Description("allow request ")] A,
        [Description("review request")] R,
        [Description("unknown")] U,
    }

    public class IovationResultType : EnumStringType<IovationResult> { }

    public class FraudIovation {
        public virtual int FraudIovationID { get; set; }
        public virtual int CustomerID { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual string Origin { get; set; }
        public virtual IovationResult Result { get; set; }
        public virtual string Reason { get; set; }
        public virtual string TrackingNumber { get; set; }
        public virtual string Score { get; set; }
        public virtual string Details { get; set; }
    }

    public sealed class FraudIovationMap : ClassMap<FraudIovation> {
        public FraudIovationMap() {
            Id(x => x.FraudIovationID).GeneratedBy.Identity();
            Map(x => x.CustomerID);
            Map(x => x.Created).CustomType<UtcDateTimeType>();
            Map(x => x.Origin).Length(20);
            Map(x => x.Result).CustomType<IovationResultType>();
            Map(x => x.Reason).Length(40);
            Map(x => x.TrackingNumber).Length(40);
            Map(x => x.Score).Length(10);
            Map(x => x.Details).LazyLoad();
        }
    }

    public class FraudIovationRepository : NHibernateRepositoryBase<FraudIovation> {
        public FraudIovationRepository(ISession session)
            : base(session) {
        }
    }
}