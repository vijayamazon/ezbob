namespace EZBob.DatabaseLib.Model.Database.Broker {
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public class BrokerRepository : NHibernateRepositoryBase<Broker> {
		public BrokerRepository(ISession session) : base(session) { } // constructor

		public Broker GetByID(int id) {
			return GetAll().FirstOrDefault(broker => broker.ID == id);
		} // GetByID
	} // class BrokerRepository
} // namespace EZBob.DatabaseLib.Model.Database.Broker
