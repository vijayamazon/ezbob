using System;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class AnalysisFunctionTimePeriodRepository : NHibernateRepositoryBase<MP_AnalysisFunctionTimePeriod>
	{
		public AnalysisFunctionTimePeriodRepository(ISession session)
			: base(session)
		{
		}

		public MP_AnalysisFunctionTimePeriod Get( Guid timePeriodInternalId )
		{
		    return EnsureTransaction(() =>
		        {
                    return GetAll()
                        .Where(p => p.InternalId == timePeriodInternalId)
                        .Cacheable()
                        .CacheRegion("LongTerm")
                        .FirstOrDefault();
		        });
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