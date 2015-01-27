using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public interface IPerformencePerUnderwriterReportRepository : IRepository<PerformencePerUnderwriterDataRow>
    {
        IList<PerformencePerUnderwriterDataRow> GetData(DateTime start, DateTime end);
    }

    public class PerformencePerUnderwriterReportRepository : NHibernateRepositoryBase<PerformencePerUnderwriterDataRow> , IPerformencePerUnderwriterReportRepository
    {
        public PerformencePerUnderwriterReportRepository(ISession session) : base(session)
        {
        }

        public IList<PerformencePerUnderwriterDataRow> GetData(DateTime start, DateTime end)
        {
            return Session.GetNamedQuery("PerformencePerUnderwriterReport")
                .SetParameter("dateStart", start)
                .SetParameter("dateEnd", end)
                .List<PerformencePerUnderwriterDataRow>();

        }
    }
}
