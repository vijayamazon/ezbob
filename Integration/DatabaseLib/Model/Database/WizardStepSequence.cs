using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {
	public class WizardStepSequence {
		public virtual int ID { get; set; }
		public virtual string StepName { get; set; }
		public virtual int? OnlineProgressBarPct { get; set; }
		public virtual int? OfflineProgressBarPct { get; set; }
		public virtual int StepType { get; set; }
	} // class WizardStepSequence

	public class WizardStepSequenceRepository : NHibernateRepositoryBase<WizardStepSequence> {
		public WizardStepSequenceRepository(ISession session) : base(session) {} // constructor
	} // class WizardStepSequenceRepository
} // namespace

namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;

	public class WizardStepSequenceMap : ClassMap<WizardStepSequence> {
		public WizardStepSequenceMap() {
			Table("WizardStepSequence");
			ReadOnly();

			Id(x => x.ID);
			Map(x => x.StepName).Length(64);
			Map(x => x.OnlineProgressBarPct);
			Map(x => x.OfflineProgressBarPct);
			Map(x => x.StepType, "WizardStepType").CustomType(typeof (WizardStepType));
		} // constructor
	} // class WizardStepSequenceMap
} // namespace
