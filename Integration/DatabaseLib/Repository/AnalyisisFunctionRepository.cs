using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.SqlCommand;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class AnalyisisFunctionRepository : NHibernateRepositoryBase<MP_AnalyisisFunction>
	{
		public AnalyisisFunctionRepository( ISession session )
			: base( session )
		{
		}

		public MP_AnalyisisFunction Get( Guid functionInternalId ) {
			return GetAll().FirstOrDefault(x => x.InternalId == functionInternalId);
		}

		public bool Exists( Guid internalId ) {
			return GetAll().Any(x => x.InternalId == internalId);
		}

	    public IList<MP_AnalyisisFunction> GetAllFunctionsAndInit()
	    {
	        return _session.CreateCriteria<MP_AnalyisisFunction>()
                    .CreateAlias("ValueType", "vt", JoinType.InnerJoin)
	                .List<MP_AnalyisisFunction>();
	    }
	}
}