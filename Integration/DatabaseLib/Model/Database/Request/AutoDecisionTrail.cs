namespace EZBob.DatabaseLib.Model.Database.Request {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ApplicationMng.Repository;
    using FluentNHibernate.Mapping;
    using Iesi.Collections.Generic;
    using NHibernate;
    using NHibernate.Type;

    public class AutoDecisionTrail {
        public AutoDecisionTrail() {
            Traces = new HashedSet<AutoDecisionTrace>();
        }
        
        public virtual long TrailID { get; set; }
        public virtual int CustomerID { get; set; }
        public virtual string TrailNotes { get; set; }
        public virtual DateTime DecisionTime { get; set; }
        public virtual string DecisionName { get; set; }
        public virtual string InputData { get; set; }
        public virtual string DecisionStatus { get; set; }
        public virtual Iesi.Collections.Generic.ISet<AutoDecisionTrace> Traces { get; set; }
    }

    public class AutoDecisionTrailMap : ClassMap<AutoDecisionTrail> {
        public AutoDecisionTrailMap() {

            Table("AutoDecisionTrail");
            ReadOnly();
            Id(x => x.TrailID);
            Map(x => x.CustomerID);
            Map(x => x.TrailNotes);
            Map(x => x.DecisionName);
            Map(x => x.DecisionTime).CustomType<UtcDateTimeType>(); ;
            Map(x => x.InputData);
            Map(x => x.DecisionStatus);

            HasMany(x => x.Traces)
                .KeyColumn("TrailID")
                .Inverse()
                .Cascade.All();

        } // constructor
    }

    public class AutoDecisionTrailRepository : NHibernateRepositoryBase<AutoDecisionTrail> {
        public AutoDecisionTrailRepository(ISession session) : base(session) { } // constructor

        public IEnumerable<AutoDecisionTrail> GetForCustomer(int customerID) {
            return GetAll().Where(x => x.CustomerID == customerID).OrderByDescending(x => x.TrailID);
        }
    }

}
