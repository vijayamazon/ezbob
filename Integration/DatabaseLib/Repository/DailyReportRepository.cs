using System;
using System.Collections.Generic;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
    public interface IDailyReportRepository : IRepository<DailyReportRow>
    {
        IList<DailyReportRow> GetData(DateTime date);
    }

    public class DailyReportRepository : NHibernateRepositoryBase<DailyReportRow>, IDailyReportRepository
    {
        public DailyReportRepository(ISession session) : base(session)
        {
        }

        public IList<DailyReportRow> GetData(DateTime date)
        {
            return Session.GetNamedQuery("GetDailyReport")
                .SetParameter("date", date)
                .List<DailyReportRow>();
        }
    }
}
