namespace EZBob.DatabaseLib.Model.Database.Broker {
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public class BrokerRepository : NHibernateRepositoryBase<Broker> {

		public BrokerRepository(ISession session) : base(session) { } // constructor

		public Broker Find(string sContactEmail) {
			return GetAll().FirstOrDefault(x => x.ContactEmail == sContactEmail);
		} // Find

		public Broker GetByUserId(int userId) {
			return GetAll().FirstOrDefault(x => x.ID == userId);
		} // Find

	} // class BrokerRepository
} // namespace EZBob.DatabaseLib.Model.Database.Broker
