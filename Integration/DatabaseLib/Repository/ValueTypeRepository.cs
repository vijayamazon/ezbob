using System;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class ValueTypeRepository : NHibernateRepositoryBase<MP_ValueType>
	{
		public ValueTypeRepository( ISession session )
			: base( session )
		{
		}

		public MP_ValueType Get( Guid internalMarketPlaceId )
		{
			foreach ( var marketPlace in GetAll() )
			{
				if ( marketPlace.InternalId.Equals( internalMarketPlaceId ) )
				{
					return marketPlace;
				}
			}

			return null;
		}

		public bool Exists( Guid internalId )
		{
			foreach ( var marketPlace in GetAll() )
			{
				if ( marketPlace.InternalId.Equals( internalId ) )
				{
					return true;
				}
			}

			return false;
		}
	}
}