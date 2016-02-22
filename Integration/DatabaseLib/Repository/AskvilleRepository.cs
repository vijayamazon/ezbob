namespace EZBob.DatabaseLib.Repository {
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using NHibernate;

	public class AskvilleRepository : NHibernateRepositoryBase<Askville> {
		public AskvilleRepository(ISession session) : base(session) {
		} // constructor

		public Askville GetAskvilleByGuid(string guid) {
			return GetAll().FirstOrDefault(x => x.Guid == guid);
		} // GetAskvilleByGuid

		public Askville GetAskvilleByMarketplace(MP_CustomerMarketPlace customerMarketPlace) {
			return GetAll().FirstOrDefault(x => x.MarketPlace.Id == customerMarketPlace.Id);
		} // GetAskvilleByMarketplace
	} // class AskvilleRepository
} // namespace
