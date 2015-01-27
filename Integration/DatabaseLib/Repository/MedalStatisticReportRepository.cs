using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public interface IMedalStatisticReportRepository : IRepository<MedalStatisticDataRow>
    {
        IList<MedalStatisticDataRow> GetData(DateTime start, DateTime end);
    }

    public class MedalStatisticReportRepository : NHibernateRepositoryBase<MedalStatisticDataRow>, IMedalStatisticReportRepository
    {
        public MedalStatisticReportRepository(ISession session) : base(session)
        {
        }

        public IList<MedalStatisticDataRow> GetData(DateTime start, DateTime end)
        {
            return Session.GetNamedQuery("GetMedalStatisticReport")
                .SetParameter("dateStart", start)
                .SetParameter("dateEnd", end)
                .List<MedalStatisticDataRow>();
        }
    }
}
