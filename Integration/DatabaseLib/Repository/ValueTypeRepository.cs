using System;
using System.Linq;
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
		    return GetAll().SingleOrDefault(x => x.InternalId == internalMarketPlaceId);
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