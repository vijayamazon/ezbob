namespace EZBob.DatabaseLib.Repository
{
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Model.Database;
	using NHibernate;
	using NHibernate.Linq;

	public class AnalyisisFunctionValueRepository : NHibernateRepositoryBase<MP_AnalyisisFunctionValue>
	{
		public AnalyisisFunctionValueRepository(ISession session)
			: base(session)
		{
		}

		public MP_AnalyisisFunctionValue Get( MP_CustomerMarketPlace customerMarketPlace, MP_AnalyisisFunction function, MP_AnalysisFunctionTimePeriod timePeriod )
		{
			return _session.Query<MP_AnalyisisFunctionValue>().FirstOrDefault( functionValue => functionValue.CustomerMarketPlace.Id == customerMarketPlace.Id && functionValue.AnalyisisFunction.Id == function.Id && functionValue.AnalysisFunctionTimePeriod.Id == timePeriod.Id);
		}

	    public IList<MP_AnalyisisFunctionValue> GetAllValuesFor(MP_CustomerMarketPlace mpCustomerMarketPlace)
	    {
	        return _session
	            .QueryOver<MP_AnalyisisFunctionValue>()
	            .Where(v => v.CustomerMarketPlace.Id == mpCustomerMarketPlace.Id)
	            //.Fetch(v => v.AnalyisisFunction).Eager
	            .List<MP_AnalyisisFunctionValue>();
	    }
	}
}