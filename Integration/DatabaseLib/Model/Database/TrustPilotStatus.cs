namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;
	using ApplicationMng.Repository;
	using NHibernate;

	public class TrustPilotStatus {
		public virtual int ID { get; set; }
		public virtual string Name { get; set; }
		public virtual string Description { get; set; }

		public virtual bool IsMe(TrustPilotStauses nStatus) {
			return nStatus.ToString() == Name;
		} // IsMe

	} // class TrustPilotStatus

	public class TrustPilotStatusMap : ClassMap<TrustPilotStatus> {
		public TrustPilotStatusMap() {
			Table("TrustPilotStatus");
			Cache.ReadOnly().Region("LongTerm").ReadOnly();

			Id(x => x.ID, "TrustPilotStatusID");
			Map(x => x.Name, "TrustPilotStatus").Length(32);
			Map(x => x.Description, "TrustPilotStatusDescription").Length(255);
		} // constructor
	} // class TrustPilotStatusMap

	public class TrustPilotStatusRepository : NHibernateRepositoryBase<TrustPilotStatus> {
		public TrustPilotStatusRepository(ISession session) : base(session) {} // constructor

		public TrustPilotStatus Find(TrustPilotStauses nStatus) {
			foreach (TrustPilotStatus tps in GetAll())
				if (tps.IsMe(nStatus))
					return tps;

			return null;
		} // Find
	} // class TrustPilotStatusRepository
} // namespace EZBob.DatabaseLib.Model.Database
