using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {
	public class WizardStepSequence {
		public virtual int ID { get; set; }
		public virtual int? OnlineProgressBarPct { get; set; }
		public virtual int? OfflineProgressBarPct { get; set; }
		public virtual WizardStep WizardStep { get; set; }

		public virtual string Name() {
			return WizardStep.Name;
		} // Name

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
			Map(x => x.OnlineProgressBarPct);
			Map(x => x.OfflineProgressBarPct);
			References(x => x.WizardStep, "WizardStepType");
		} // constructor
	} // class WizardStepSequenceMap
} // namespace
