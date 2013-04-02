using System;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class MarketPlaceRepository : NHibernateRepositoryBase<MP_MarketplaceType>
	{
		public MarketPlaceRepository( ISession session )
			: base( session )
		{
		}

		public MP_MarketplaceType Get( Guid internalMarketPlaceId )
		{
			foreach ( var marketPlace in GetAll() )
			{
				if ( marketPlace.InternalId.Equals( internalMarketPlaceId ) )
				{
					return marketPlace;
				}
			}

			return null;
			/*var a = GetAll();
		    return _session
                    .QueryOver<MP_MarketplaceType>()
                    .Where(mp => mp.InternalId == internalMarketPlaceId)
                    .CacheMode(CacheMode.Normal)
                    .Cacheable()
                    .CacheRegion("MarketPlaces")
                    .SingleOrDefault<MP_MarketplaceType>();*/
		}

		public MP_MarketplaceType Get( int clientId )
		{
			return Get( clientId as object);
		}

		public bool Exists( Guid internalMarketPlaceId )
		{
		    return Get(internalMarketPlaceId) != null;
		}
	}
}