namespace EZBob.DatabaseLib.Model.Database.Repository {
	using System;
	using ApplicationMng.Repository;
	using NHibernate;

	public class MarketPlaceRepository : NHibernateRepositoryBase<MP_MarketplaceType> {
		public MarketPlaceRepository(ISession session)
			: base(session) {
		}

		public bool Exists(Guid internalMarketPlaceId) {
			return Get(internalMarketPlaceId) != null;
		}

		public MP_MarketplaceType Get(Guid internalMarketPlaceId) {
			return _session
					.QueryOver<MP_MarketplaceType>()
					.Where(mp => mp.InternalId == internalMarketPlaceId)
					.CacheMode(CacheMode.Normal)
					.Cacheable()
					.CacheRegion("MarketPlaces")
					.SingleOrDefault<MP_MarketplaceType>();
		}

		public MP_MarketplaceType Get(int clientId) {
			return Get(clientId as object);
		}
	}
}