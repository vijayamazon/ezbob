namespace EZBob.DatabaseLib.Model.Database.Broker {
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public class BrokerRepository : NHibernateRepositoryBase<Broker> {
		#region constructor

		public BrokerRepository(ISession session) : base(session) { } // constructor

		#endregion constructor

		#region method Find

		public Broker Find(string sContactEmail) {
			return GetAll().FirstOrDefault(x => x.ContactEmail == sContactEmail);
		} // Find

		public Broker GetByUserId(int userId)
		{
			return GetAll().FirstOrDefault(x => x.UserID == userId);
		} // Find
		#endregion method Find
	} // class BrokerRepository
} // namespace EZBob.DatabaseLib.Model.Database.Broker
