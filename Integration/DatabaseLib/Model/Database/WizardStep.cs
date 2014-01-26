using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {
	public class WizardStep {
		public virtual int ID { get; set; }
		public virtual bool TheLastOne { get; set; }
		public virtual string Name { get; set; }
		public virtual string Description { get; set; }

		public override string ToString() {
			return string.Format(
				"{0} ({1}) <{2}> {3}",
				Name,
				ID,
				TheLastOne ? "final" : "not final",
				Description
			);
		} // ToString
	} // class WizardStep

	public class WizardStepRepository : NHibernateRepositoryBase<WizardStep> {
		public WizardStepRepository(ISession session) : base(session) {} // constructor
	} // class WizardStepRepository
} // namespace

namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;

	public class WizardStepMap : ClassMap<WizardStep> {
		public WizardStepMap() {
			Table("WizardStepTypes");
			ReadOnly();

			Id(x => x.ID, "WizardStepTypeID");
			Map(x => x.TheLastOne);
			Map(x => x.Name, "WizardStepTypeName").Length(64);
			Map(x => x.Description, "WizardStepTypeDescription").Length(255);
		} // constructor
	} // class WizardStepSequenceMap
} // namespace
