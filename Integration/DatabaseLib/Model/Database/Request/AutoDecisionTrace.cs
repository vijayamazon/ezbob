namespace EZBob.DatabaseLib.Model.Database.Request {
    using ApplicationMng.Repository;
    using FluentNHibernate.Mapping;
    using NHibernate;

    public class AutoDecisionTrace {
        public virtual long TraceID { get; set; }
        public virtual long TrailID { get; set; }
        public virtual int Position { get; set; }
        public virtual string Comment { get; set; }
        public virtual string TraceName { get; set; }
        public virtual string DecisionStatus { get; set; }
        public virtual bool HasLockedDecision { get; set; }
    }

    public class AutoDecisionTraceMap : ClassMap<AutoDecisionTrace> {
        public AutoDecisionTraceMap() {

            Table("AutoDecisionTrace");
            ReadOnly();
            Id(x => x.TraceID);
            Map(x => x.TrailID);
            Map(x => x.Position);
            Map(x => x.Comment);
            Map(x => x.TraceName);
            Map(x => x.DecisionStatus);
            Map(x => x.HasLockedDecision);
        } // constructor
    }

    public class AutoDecisionTraceRepository : NHibernateRepositoryBase<AutoDecisionTrace> {
        public AutoDecisionTraceRepository(ISession session) : base(session) { } // constructor
    }

}
