using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;
using NHibernate.SqlCommand;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class AnalyisisFunctionRepository : NHibernateRepositoryBase<MP_AnalyisisFunction>
	{
		public AnalyisisFunctionRepository( ISession session )
			: base( session )
		{
		}

		public MP_AnalyisisFunction Get( Guid functionInternalId )
		{
			foreach ( var marketPlace in GetAll() )
			{
				if ( marketPlace.InternalId.Equals( functionInternalId ) )
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

	    public IList<MP_AnalyisisFunction> GetAllFunctionsAndInit()
	    {
	        return _session.CreateCriteria<MP_AnalyisisFunction>()
                    .CreateAlias("ValueType", "vt", JoinType.InnerJoin)
	                .List<MP_AnalyisisFunction>();
	    }
	}
}